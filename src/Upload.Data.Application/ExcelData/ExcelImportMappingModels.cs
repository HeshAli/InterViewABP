using System;

namespace Upload.Data.ExcelData;

public sealed record CreateExcelImportBatchMapInput(
    Guid Id,
    Guid? TenantId,
    string FileName,
    Guid UploadedByUserId,
    DateTime UploadTimeUtc
);

public sealed record CreateExcelDataRowMapInput(
    Guid Id,
    Guid BatchId,
    Guid UploadedByUserId,
    string ColumnA,
    string ColumnB,
    string ColumnC,
    decimal NumericValue
);
