using Volo.Abp.Modularity;

namespace Upload.Data;

[DependsOn(
    typeof(UploadFileDomainModule),
    typeof(UploadFileTestBaseModule)
)]
public class UploadFileDomainTestModule : AbpModule
{

}

