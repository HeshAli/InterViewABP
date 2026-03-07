using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Upload.Data.Data;

/* This is used if database provider does't define
 * IUploadFileDbSchemaMigrator implementation.
 */
public class NullUploadFileDbSchemaMigrator : IUploadFileDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}

