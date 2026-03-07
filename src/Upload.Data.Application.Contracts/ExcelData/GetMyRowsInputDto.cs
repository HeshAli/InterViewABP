using Volo.Abp.Application.Dtos;

namespace Upload.Data.ExcelData;

public class GetMyRowsInputDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 3;
    public const int MaxAllowedPageSize = 100;

    public string? Filter { get; set; }

    public GetMyRowsInputDto()
    {
        MaxResultCount = DefaultPageSize;
        Sorting = "CreationTime desc";
    }
}

