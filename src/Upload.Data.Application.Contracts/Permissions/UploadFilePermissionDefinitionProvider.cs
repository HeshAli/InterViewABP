using Upload.Data.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Upload.Data.Permissions;

public class UploadFilePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(UploadFilePermissions.GroupName, L("Permission:UploadFile"));

        var dataUpload = group.AddPermission(UploadFilePermissions.DataUpload.Default, L("Permission:DataUpload"));
        dataUpload.AddChild(UploadFilePermissions.DataUpload.DataUploading, L("Permission:DataUpload.DataUploading"));
        dataUpload.AddChild(UploadFilePermissions.DataUpload.UploadedData, L("Permission:DataUpload.UploadedData"));

        group.AddPermission(UploadFilePermissions.Dashboard.Default, L("Permission:Dashboard"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<UploadFileResource>(name);
    }
}

