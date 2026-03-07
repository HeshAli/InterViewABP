using System.Globalization;
using System.Threading.Tasks;
using Test1.BookStore.Settings;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;

namespace Test1.BookStore;

public class BookStoreDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ISettingManager _settingManager;

    public BookStoreDataSeederContributor(ISettingManager settingManager)
        => _settingManager = settingManager;

    public async Task SeedAsync(DataSeedContext context)
    {
        // Keep business: seed global defaults if missing
        await SeedExcelDataSettingsAsync();
    }

    private async Task SeedExcelDataSettingsAsync()
    {
        var seeds = new (string Name, string Value)[]
        {
            (BookStoreSettings.ExcelData.MaxUploadBytes,
                BookStoreSettings.ExcelData.DefaultMaxUploadBytes.ToString(CultureInfo.InvariantCulture)),

            (BookStoreSettings.ExcelData.MaxAllowedRows,
                BookStoreSettings.ExcelData.DefaultMaxAllowedRows.ToString(CultureInfo.InvariantCulture)),

            (BookStoreSettings.ExcelData.MaxAllowedSharedStrings,
                BookStoreSettings.ExcelData.DefaultMaxAllowedSharedStrings.ToString(CultureInfo.InvariantCulture)),

            (BookStoreSettings.ExcelData.MaxChartItems,
                BookStoreSettings.ExcelData.DefaultMaxChartItems.ToString(CultureInfo.InvariantCulture))
        };

        foreach (var (name, value) in seeds)
        {
            await SeedGlobalSettingIfMissingAsync(name, value);
        }
    }

    private async Task SeedGlobalSettingIfMissingAsync(string name, string value)
    {
        var currentValue = await _settingManager.GetOrNullGlobalAsync(name);

        // if it exists (even whitespace trimmed), skip
        if (!string.IsNullOrWhiteSpace(currentValue?.Trim()))
            return;

        await _settingManager.SetGlobalAsync(name, value);
    }
}