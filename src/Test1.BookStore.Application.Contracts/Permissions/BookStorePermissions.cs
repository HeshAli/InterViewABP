namespace Test1.BookStore.Permissions;

public static class BookStorePermissions
{
    public const string GroupName = "BookStore";

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
