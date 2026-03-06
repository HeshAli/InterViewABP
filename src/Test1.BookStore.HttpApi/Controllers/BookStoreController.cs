using Test1.BookStore.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Test1.BookStore.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class BookStoreController : AbpControllerBase
{
    protected BookStoreController()
    {
        LocalizationResource = typeof(BookStoreResource);
    }
}
