using Test1.BookStore.Localization;
using Volo.Abp.Application.Services;

namespace Test1.BookStore;

/* Inherit your application services from this class.
 */
public abstract class BookStoreAppService : ApplicationService
{
    protected BookStoreAppService()
    {
        LocalizationResource = typeof(BookStoreResource);
    }
}
