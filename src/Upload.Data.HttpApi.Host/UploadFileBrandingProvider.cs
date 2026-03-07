using Microsoft.Extensions.Localization;
using Upload.Data.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Upload.Data;

[Dependency(ReplaceServices = true)]
public class UploadFileBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<UploadFileResource> _localizer;

    public UploadFileBrandingProvider(IStringLocalizer<UploadFileResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}

