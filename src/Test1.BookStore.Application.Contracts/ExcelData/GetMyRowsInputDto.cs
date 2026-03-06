using Volo.Abp.Application.Dtos;

namespace Test1.BookStore.ExcelData;

public class GetMyRowsInputDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 3;
    public const int MaxAllowedPageSize = 100;

    public GetMyRowsInputDto()
    {
        MaxResultCount = DefaultPageSize;
        Sorting = "CreationTime desc";
    }
}

