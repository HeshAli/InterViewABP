using Volo.Abp.Modularity;

namespace Test1.BookStore;

public abstract class BookStoreApplicationTestBase<TStartupModule> : BookStoreTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
