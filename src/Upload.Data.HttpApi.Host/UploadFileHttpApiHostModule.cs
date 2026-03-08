using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Upload.Data.EntityFrameworkCore;
using Upload.Data.Filters;
using Upload.Data.HealthChecks;
using Upload.Data.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Studio;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Upload.Data;

[DependsOn(
    typeof(UploadFileHttpApiModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(UploadFileApplicationModule),
    typeof(UploadFileEntityFrameworkCoreModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule)
    )]
public class UploadFileHttpApiHostModule : AbpModule
{
    private const string MicrosoftScheme = "Microsoft";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("UploadFile");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });

            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        ConfigureAuthentication(context);
        context.Services.AddTransient<LoginFailedMessagePageFilter>();
        Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<LoginFailedMessagePageFilter>();
        });
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureConventionalControllers();
        ConfigureHealthChecks(context);
        ConfigureSwagger(context, configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });

        ConfigureMicrosoftAuthentication(context, configuration);
    }

    private static void ConfigureMicrosoftAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var clientId = configuration["Authentication:Microsoft:ClientId"];
        var clientSecret = configuration["Authentication:Microsoft:ClientSecret"];
        var authorityOrTenant = configuration["Authentication:Microsoft:Authority"];
        if (string.IsNullOrWhiteSpace(authorityOrTenant))
        {
            authorityOrTenant = configuration["Authentication:Microsoft:TenantId"];
        }

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return;
        }

        var authorityTenant = ResolveMicrosoftAuthorityTenant(authorityOrTenant);
        var authority = $"https://login.microsoftonline.com/{authorityTenant}/oauth2/v2.0";

        context.Services
            .AddAuthentication()
            .AddOAuth(MicrosoftScheme, MicrosoftScheme, options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = "/signin-microsoft";
                options.AuthorizationEndpoint = $"{authority}/authorize";
                options.TokenEndpoint = $"{authority}/token";
                options.UserInformationEndpoint = "https://graph.microsoft.com/v1.0/me?$select=id,displayName,mail,userPrincipalName";
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("offline_access");
                options.Scope.Add("User.Read");
                options.SaveTokens = true;
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "displayName");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "mail");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = CreateMicrosoftTicketAsync,
                };
            });
    }

    private static string ResolveMicrosoftAuthorityTenant(string? configuredTenantId)
    {
        if (string.IsNullOrWhiteSpace(configuredTenantId))
        {
            // Host-side ABP tenant (TenantId = null) is unrelated to Entra tenant selection.
            // Default to organizations to avoid consumers-only errors for single-tenant apps.
            return "organizations";
        }

        var tenant = configuredTenantId.Trim();

        if (tenant.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase) &&
            !tenant.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !tenant.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            tenant = $"https://{tenant.TrimStart('/')}";
        }

        if (tenant.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            tenant.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(tenant, UriKind.Absolute, out var tenantUri))
            {
                var pathSegments = tenantUri.AbsolutePath
                    .Trim('/')
                    .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                tenant = pathSegments.FirstOrDefault() ?? string.Empty;
            }
        }
        else if (tenant.Contains('/'))
        {
            var pathSegments = tenant
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            tenant = pathSegments.FirstOrDefault() ?? string.Empty;
        }

        tenant = tenant.Trim().Trim('/');

        if (string.IsNullOrWhiteSpace(tenant) ||
            tenant.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            tenant.Equals("v2.0", StringComparison.OrdinalIgnoreCase) ||
            tenant.Equals("oauth2", StringComparison.OrdinalIgnoreCase))
        {
            return "organizations";
        }

        if (tenant.Equals("common", StringComparison.OrdinalIgnoreCase) ||
            tenant.Equals("organizations", StringComparison.OrdinalIgnoreCase) ||
            tenant.Equals("consumers", StringComparison.OrdinalIgnoreCase))
        {
            return tenant.ToLowerInvariant();
        }

        return tenant;
    }

    private static async Task CreateMicrosoftTicketAsync(OAuthCreatingTicketContext context)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

        using var response = await context.Backchannel.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            context.HttpContext.RequestAborted
        );
        response.EnsureSuccessStatusCode();

        await using var userStream = await response.Content.ReadAsStreamAsync(context.HttpContext.RequestAborted);
        using var userPayload = await JsonDocument.ParseAsync(userStream, cancellationToken: context.HttpContext.RequestAborted);

        context.RunClaimActions(userPayload.RootElement);

        if (context.Identity?.FindFirst(ClaimTypes.Email) is null &&
            userPayload.RootElement.TryGetProperty("userPrincipalName", out var userPrincipalNameValue))
        {
            var userPrincipalName = userPrincipalNameValue.GetString();
            if (!string.IsNullOrWhiteSpace(userPrincipalName))
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.Email, userPrincipalName));
            }
        }
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.Applications["Angular"].RootUrl = configuration["App:AngularUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                }
            );
        });
    }


    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<UploadFileDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Upload.Data.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<UploadFileDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Upload.Data.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<UploadFileApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Upload.Data.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<UploadFileApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Upload.Data.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(UploadFileApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOidc(
            configuration["AuthServer:Authority"]!,
            ["UploadFile"],
            [AbpSwaggerOidcFlows.AuthorizationCode],
            null,
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "UploadFile API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddUploadFileHealthChecks();
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseRouting();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseAbpSecurityHeaders();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "UploadFile API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}


