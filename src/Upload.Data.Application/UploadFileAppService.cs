using Upload.Data.Localization;
using Volo.Abp.Application.Services;

namespace Upload.Data;

/* Inherit your application services from this class.
 */
public abstract class UploadFileAppService : ApplicationService
{
    protected UploadFileAppService()
    {
        LocalizationResource = typeof(UploadFileResource);
    }
}

