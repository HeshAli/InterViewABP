using Test1.BookStore.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Test1.BookStore.Permissions;

public class BookStorePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(BookStorePermissions.GroupName, L("Permission:BookStore"));

        var dataUpload = group.AddPermission(BookStorePermissions.DataUpload.Default, L("Permission:DataUpload"));
        dataUpload.AddChild(BookStorePermissions.DataUpload.DataUploading, L("Permission:DataUpload.DataUploading"));
        dataUpload.AddChild(BookStorePermissions.DataUpload.UploadedData, L("Permission:DataUpload.UploadedData"));

        group.AddPermission(BookStorePermissions.Dashboard.Default, L("Permission:Dashboard"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BookStoreResource>(name);
    }
}
