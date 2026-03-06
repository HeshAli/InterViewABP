using System;
using Volo.Abp.Application.Dtos;

namespace Test1.BookStore.ExcelData;

public class ExcelUploadResultDto : EntityDto<Guid>
{
    public string FileName { get; set; } = string.Empty;

    public DateTime UploadTimeUtc { get; set; }

    public int TotalRows { get; set; }
}