using System.Globalization;
using System.Threading.Tasks;
using Upload.Data.Settings;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;

namespace Upload.Data;

public class UploadFileDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ISettingManager _settingManager;

    public UploadFileDataSeederContributor(ISettingManager settingManager)
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
            (UploadFileSettings.ExcelData.MaxUploadBytes,
                UploadFileSettings.ExcelData.DefaultMaxUploadBytes.ToString(CultureInfo.InvariantCulture)),

            (UploadFileSettings.ExcelData.MaxAllowedRows,
                UploadFileSettings.ExcelData.DefaultMaxAllowedRows.ToString(CultureInfo.InvariantCulture)),

            (UploadFileSettings.ExcelData.MaxAllowedSharedStrings,
                UploadFileSettings.ExcelData.DefaultMaxAllowedSharedStrings.ToString(CultureInfo.InvariantCulture)),

            (UploadFileSettings.ExcelData.MaxChartItems,
                UploadFileSettings.ExcelData.DefaultMaxChartItems.ToString(CultureInfo.InvariantCulture))
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
