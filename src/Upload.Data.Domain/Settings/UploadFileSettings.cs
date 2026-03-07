namespace Upload.Data.Settings;

public static class UploadFileSettings
{
    private const string Prefix = "UploadFile";

    public static class ExcelData
    {
        private const string ExcelDataPrefix = Prefix + ".ExcelData";

        public const string MaxUploadBytes = ExcelDataPrefix + ".MaxUploadBytes";
        public const string MaxAllowedRows = ExcelDataPrefix + ".MaxAllowedRows";
        public const string MaxAllowedSharedStrings = ExcelDataPrefix + ".MaxAllowedSharedStrings";
        public const string MaxChartItems = ExcelDataPrefix + ".MaxChartItems";

        public const long DefaultMaxUploadBytes = 10 * 1024 * 1024;
        public const int DefaultMaxAllowedRows = 20_000;
        public const int DefaultMaxAllowedSharedStrings = 200_000;
        public const int DefaultMaxChartItems = 10;
    }
}

