using System.Globalization;
using Upload.Data.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace Upload.Data.Settings;

public class UploadFileSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                UploadFileSettings.ExcelData.MaxUploadBytes,
                UploadFileSettings.ExcelData.DefaultMaxUploadBytes.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<UploadFileResource>("DisplayName:UploadFile.ExcelData.MaxUploadBytes"),
                description: LocalizableString.Create<UploadFileResource>("Description:UploadFile.ExcelData.MaxUploadBytes")
            ),
            new SettingDefinition(
                UploadFileSettings.ExcelData.MaxAllowedRows,
                UploadFileSettings.ExcelData.DefaultMaxAllowedRows.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<UploadFileResource>("DisplayName:UploadFile.ExcelData.MaxAllowedRows"),
                description: LocalizableString.Create<UploadFileResource>("Description:UploadFile.ExcelData.MaxAllowedRows")
            ),
            new SettingDefinition(
                UploadFileSettings.ExcelData.MaxAllowedSharedStrings,
                UploadFileSettings.ExcelData.DefaultMaxAllowedSharedStrings.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<UploadFileResource>("DisplayName:UploadFile.ExcelData.MaxAllowedSharedStrings"),
                description: LocalizableString.Create<UploadFileResource>("Description:UploadFile.ExcelData.MaxAllowedSharedStrings")
            ),
            new SettingDefinition(
                UploadFileSettings.ExcelData.MaxChartItems,
                UploadFileSettings.ExcelData.DefaultMaxChartItems.ToString(CultureInfo.InvariantCulture),
                displayName: LocalizableString.Create<UploadFileResource>("DisplayName:UploadFile.ExcelData.MaxChartItems"),
                description: LocalizableString.Create<UploadFileResource>("Description:UploadFile.ExcelData.MaxChartItems")
            )
        );
    }
}

