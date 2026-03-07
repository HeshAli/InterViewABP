using Volo.Abp.Modularity;

namespace Upload.Data;

[DependsOn(
    typeof(UploadFileApplicationModule),
    typeof(UploadFileDomainTestModule)
)]
public class UploadFileApplicationTestModule : AbpModule
{

}

