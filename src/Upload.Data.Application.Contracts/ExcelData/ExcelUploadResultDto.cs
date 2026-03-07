using System;
using Volo.Abp.Application.Dtos;

namespace Upload.Data.ExcelData;

public class ExcelUploadResultDto : EntityDto<Guid>
{
    public string FileName { get; set; } = string.Empty;

    public DateTime UploadTimeUtc { get; set; }

    public int TotalRows { get; set; }
}
