using System;
using Volo.Abp.Application.Dtos;

namespace Upload.Data.ExcelData;

public class ExcelDataRowDto : EntityDto<Guid>
{
    public Guid BatchId { get; set; }

    public string ColumnA { get; set; } = string.Empty;

    public string ColumnB { get; set; } = string.Empty;

    public string ColumnC { get; set; } = string.Empty;

    public decimal NumericValue { get; set; }

    public DateTime CreationTime { get; set; }
}
