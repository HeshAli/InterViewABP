using Upload.Data.Samples;
using Xunit;

namespace Upload.Data.EntityFrameworkCore.Domains;

[Collection(UploadFileTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<UploadFileEntityFrameworkCoreTestModule>
{

}

