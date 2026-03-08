using System;
using System.Globalization;
using System.Threading.Tasks;
using Upload.Data.Settings;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;

namespace Upload.Data;

public class UploadFileDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private const string SelfRegistrationSettingName = "Abp.Account.IsSelfRegistrationEnabled";
    private const string DisabledSelfRegistrationValue = "false";

    private readonly ISettingManager _settingManager;

    public UploadFileDataSeederContributor(ISettingManager settingManager)
        => _settingManager = settingManager;

    public async Task SeedAsync(DataSeedContext context)
    {
        // Keep business: seed global defaults if missing
        await SeedExcelDataSettingsAsync();

        // Disable account self-registration so the Register link is hidden on login screens.
        await DisableSelfRegistrationAsync();
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

    private async Task DisableSelfRegistrationAsync()
    {
        var currentValue = await _settingManager.GetOrNullGlobalAsync(SelfRegistrationSettingName);
        if (string.Equals(currentValue?.Trim(), DisabledSelfRegistrationValue, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await _settingManager.SetGlobalAsync(SelfRegistrationSettingName, DisabledSelfRegistrationValue);
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
