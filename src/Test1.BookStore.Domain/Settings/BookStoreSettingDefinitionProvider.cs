using System.Globalization;
using Test1.BookStore.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace Test1.BookStore.Settings;

public class BookStoreSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                BookStoreSettings.ExcelData.MaxUploadBytes,
                BookStoreSettings.ExcelData.DefaultMaxUploadBytes.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<BookStoreResource>("DisplayName:BookStore.ExcelData.MaxUploadBytes"),
                description: LocalizableString.Create<BookStoreResource>("Description:BookStore.ExcelData.MaxUploadBytes")
            ),
            new SettingDefinition(
                BookStoreSettings.ExcelData.MaxAllowedRows,
                BookStoreSettings.ExcelData.DefaultMaxAllowedRows.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<BookStoreResource>("DisplayName:BookStore.ExcelData.MaxAllowedRows"),
                description: LocalizableString.Create<BookStoreResource>("Description:BookStore.ExcelData.MaxAllowedRows")
            ),
            new SettingDefinition(
                BookStoreSettings.ExcelData.MaxAllowedSharedStrings,
                BookStoreSettings.ExcelData.DefaultMaxAllowedSharedStrings.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<BookStoreResource>("DisplayName:BookStore.ExcelData.MaxAllowedSharedStrings"),
                description: LocalizableString.Create<BookStoreResource>("Description:BookStore.ExcelData.MaxAllowedSharedStrings")
            ),
            new SettingDefinition(
                BookStoreSettings.ExcelData.MaxChartItems,
                BookStoreSettings.ExcelData.DefaultMaxChartItems.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<BookStoreResource>("DisplayName:BookStore.ExcelData.MaxChartItems"),
                description: LocalizableString.Create<BookStoreResource>("Description:BookStore.ExcelData.MaxChartItems")
            )
        );
    }
}
