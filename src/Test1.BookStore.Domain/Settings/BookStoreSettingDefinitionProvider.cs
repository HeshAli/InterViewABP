using Volo.Abp.Settings;

namespace Test1.BookStore.Settings;

public class BookStoreSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(BookStoreSettings.MySetting1));
    }
}
