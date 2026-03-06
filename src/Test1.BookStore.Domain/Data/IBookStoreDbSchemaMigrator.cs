using System.Threading.Tasks;

namespace Test1.BookStore.Data;

public interface IBookStoreDbSchemaMigrator
{
    Task MigrateAsync();
}
