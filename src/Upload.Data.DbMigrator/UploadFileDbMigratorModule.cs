using Upload.Data.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Upload.Data.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(UploadFileEntityFrameworkCoreModule),
    typeof(UploadFileApplicationContractsModule)
)]
public class UploadFileDbMigratorModule : AbpModule
{
}

