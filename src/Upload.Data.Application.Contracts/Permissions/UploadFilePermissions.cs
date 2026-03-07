namespace Upload.Data.Permissions;

public static class UploadFilePermissions
{
    public const string GroupName = "UploadFile";

    public static class DataUpload
    {
        public const string Default = GroupName + ".DataUpload";
        public const string DataUploading = Default + ".DataUploading";
        public const string UploadedData = Default + ".UploadedData";
    }

    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
    }
}

