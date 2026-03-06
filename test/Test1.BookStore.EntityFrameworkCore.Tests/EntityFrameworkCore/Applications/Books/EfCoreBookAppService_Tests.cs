using Test1.BookStore.Books;
using Xunit;

namespace Test1.BookStore.EntityFrameworkCore.Applications.Books;

[Collection(BookStoreTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<BookStoreEntityFrameworkCoreTestModule>
{

}