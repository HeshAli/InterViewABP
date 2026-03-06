using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Test1.BookStore.ExcelData;

public interface IExcelDataAppService : IApplicationService
{
    Task<ExcelUploadResultDto> UploadAsync(IRemoteStreamContent file, CancellationToken cancellationToken = default);

    Task<PagedResultDto<ExcelDataRowDto>> GetMyRowsAsync(GetMyRowsInputDto input, CancellationToken cancellationToken = default);

    Task<List<ExcelChartItemDto>> GetMyChartAsync(CancellationToken cancellationToken = default);
}

