using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Test1.BookStore.ExcelData;

public class ExcelDataRow : FullAuditedEntity<Guid>
{
    public Guid BatchId { get; private set; }

    public ExcelImportBatch Batch { get; private set; } = default!;

    public Guid UploadedByUserId { get; private set; }

    public string ColumnA { get; private set; } = string.Empty;

    public string ColumnB { get; private set; } = string.Empty;

    public string ColumnC { get; private set; } = string.Empty;

    public decimal NumericValue { get; private set; }

    protected ExcelDataRow()
    {
    }

    public ExcelDataRow(
        Guid id,
        Guid batchId,
        Guid uploadedByUserId,
        string columnA,
        string columnB,
        string columnC,
        decimal numericValue)
        : base(id)
    {
        BatchId = batchId;
        UploadedByUserId = uploadedByUserId;
        ColumnA = columnA ?? string.Empty;
        ColumnB = columnB ?? string.Empty;
        ColumnC = columnC ?? string.Empty;
        NumericValue = numericValue;
    }
}