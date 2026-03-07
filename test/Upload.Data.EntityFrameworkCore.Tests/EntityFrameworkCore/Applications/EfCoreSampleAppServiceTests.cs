using Upload.Data.Samples;
using Xunit;

namespace Upload.Data.EntityFrameworkCore.Applications;

[Collection(UploadFileTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<UploadFileEntityFrameworkCoreTestModule>
{

}

