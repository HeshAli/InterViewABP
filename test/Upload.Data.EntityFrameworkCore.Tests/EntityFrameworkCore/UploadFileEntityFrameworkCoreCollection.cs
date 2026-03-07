using Xunit;

namespace Upload.Data.EntityFrameworkCore;

[CollectionDefinition(UploadFileTestConsts.CollectionDefinitionName)]
public class UploadFileEntityFrameworkCoreCollection : ICollectionFixture<UploadFileEntityFrameworkCoreFixture>
{

}

