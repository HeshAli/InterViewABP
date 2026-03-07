using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Upload.Data.ExcelData;

public class ExcelImportBatch : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public Guid UploadedByUserId { get; private set; }

    public DateTime UploadTimeUtc { get; private set; }

    public int TotalRows { get; private set; }

    public ICollection<ExcelDataRow> Rows { get; private set; } = new List<ExcelDataRow>();

    protected ExcelImportBatch()
    {
    }

    public ExcelImportBatch(
        Guid id,
        Guid? tenantId,
        string fileName,
        Guid uploadedByUserId,
        DateTime uploadTimeUtc)
        : base(id)
    {
        TenantId = tenantId;
        FileName = fileName ?? string.Empty;
        UploadedByUserId = uploadedByUserId;
        UploadTimeUtc = uploadTimeUtc;
    }

    public void SetTotalRows(int totalRows)
    {
        TotalRows = totalRows;
    }
}
