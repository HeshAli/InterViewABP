using Volo.Abp.Modularity;

namespace Upload.Data;

public abstract class UploadFileApplicationTestBase<TStartupModule> : UploadFileTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}

