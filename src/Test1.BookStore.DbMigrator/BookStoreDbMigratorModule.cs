using Test1.BookStore.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Test1.BookStore.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(BookStoreEntityFrameworkCoreModule),
    typeof(BookStoreApplicationContractsModule)
)]
public class BookStoreDbMigratorModule : AbpModule
{
}
