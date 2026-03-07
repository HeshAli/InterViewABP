using Upload.Data.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Upload.Data.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class UploadFileController : AbpControllerBase
{
    protected UploadFileController()
    {
        LocalizationResource = typeof(UploadFileResource);
    }
}

