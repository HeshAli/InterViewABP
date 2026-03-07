using System.Threading.Tasks;

namespace Upload.Data.Data;

public interface IUploadFileDbSchemaMigrator
{
    Task MigrateAsync();
}

