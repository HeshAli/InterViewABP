using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Upload.Data.Localization;

namespace Upload.Data.Filters;

public class LoginFailedMessagePageFilter : IAsyncPageFilter
{
    private const string LoginPath = "/Account/Login";
    private readonly IStringLocalizer<UploadFileResource> _localizer;

    public LoginFailedMessagePageFilter(IStringLocalizer<UploadFileResource> localizer)
    {
        _localizer = localizer;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var executedContext = await next();

        if (executedContext.Exception != null || executedContext.Result is not PageResult)
        {
            return;
        }

        var request = context.HttpContext.Request;
        if (!HttpMethods.IsPost(request.Method) ||
            !request.Path.Equals(LoginPath, StringComparison.OrdinalIgnoreCase) ||
            !request.HasFormContentType)
        {
            return;
        }

        if (context.ModelState.ErrorCount > 0)
        {
            return;
        }

        var form = request.Form;
        var action = form["Action"].ToString();
        if (!action.Equals("Login", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var userNameOrEmail = form["LoginInput.UserNameOrEmailAddress"].ToString();
        var password = form["LoginInput.Password"].ToString();

        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        context.ModelState.AddModelError(string.Empty, _localizer["Login:InvalidCredentials"]);
    }
}
