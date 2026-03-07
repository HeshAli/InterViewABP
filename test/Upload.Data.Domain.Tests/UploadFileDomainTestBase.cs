using Volo.Abp.Modularity;

namespace Upload.Data;

/* Inherit from this class for your domain layer tests. */
public abstract class UploadFileDomainTestBase<TStartupModule> : UploadFileTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}

