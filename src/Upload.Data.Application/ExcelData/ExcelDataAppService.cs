using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Upload.Data.Permissions;
using Upload.Data.Settings;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Upload.Data.ExcelData;

[Authorize]
public class ExcelDataAppService : ApplicationService, IExcelDataAppService
{
    private readonly IRepository<ExcelImportBatch, Guid> _batchRepository;
    private readonly IRepository<ExcelDataRow, Guid> _rowRepository;
    private readonly ISettingProvider _settingProvider;

    public ExcelDataAppService(
        IRepository<ExcelImportBatch, Guid> batchRepository,
        IRepository<ExcelDataRow, Guid> rowRepository,
        ISettingProvider settingProvider)
    {
        _batchRepository = batchRepository;
        _rowRepository = rowRepository;
        _settingProvider = settingProvider;
    }

    [Authorize(UploadFilePermissions.DataUpload.DataUploading)]
    public async Task<ExcelUploadResultDto> UploadAsync(IRemoteStreamContent file, CancellationToken cancellationToken = default)
    {
        if (file == null)
            throw new UserFriendlyException(L["ExcelUpload:FileIsRequired"]);

        var fileName = file.FileName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException(L["ExcelUpload:OnlyXlsxAllowed"]);

        var limits = await GetExcelLimitsAsync();

        if (file.ContentLength.HasValue && file.ContentLength.Value > limits.MaxUploadBytes)
            throw new UserFriendlyException(L["ExcelUpload:FileTooLarge"]);

        await using var inputStream = file.GetStream();

        // Ensure we have a seekable stream for OpenXml processing.
        await using var excelStream = await EnsureSeekableAsync(
            inputStream,
            file.ContentLength,
            limits.MaxUploadBytes,
            cancellationToken
        );

        if (excelStream.Length == 0)
            throw new UserFriendlyException(L["ExcelUpload:FileIsEmpty"]);

        excelStream.Position = 0;

        List<ParsedExcelRow> parsedRows;
        try
        {
            parsedRows = ParseExcelRows(
                excelStream,
                maxRows: limits.MaxAllowedRows,
                maxSharedStrings: limits.MaxAllowedSharedStrings
            );
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch
        {
            throw new UserFriendlyException(L["ExcelUpload:InvalidExcelFile"]);
        }

        if (parsedRows.Count == 0)
            throw new UserFriendlyException(L["ExcelUpload:NoDataRowsFound"]);

        var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("User is not authenticated.");

        var batch = new ExcelImportBatch(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            fileName,
            userId,
            Clock.Now.ToUniversalTime()
        );

        await _batchRepository.InsertAsync(batch, autoSave: false, cancellationToken: cancellationToken);

        var entities = parsedRows.Select(r => new ExcelDataRow(
                GuidGenerator.Create(),
                batch.Id,
                userId,
                r.ColumnA,
                r.ColumnB,
                r.ColumnC,
                r.NumericValue
            ))
            .ToList();

        await _rowRepository.InsertManyAsync(entities, autoSave: false, cancellationToken: cancellationToken);

        batch.SetTotalRows(entities.Count);
        await _batchRepository.UpdateAsync(batch, autoSave: false, cancellationToken: cancellationToken);

        return new ExcelUploadResultDto
        {
            Id = batch.Id,
            FileName = batch.FileName,
            UploadTimeUtc = batch.UploadTimeUtc,
            TotalRows = batch.TotalRows
        };
    }

    [Authorize(UploadFilePermissions.DataUpload.UploadedData)]
    public async Task<PagedResultDto<ExcelDataRowDto>> GetMyRowsAsync(GetMyRowsInputDto input, CancellationToken cancellationToken = default)
    {
        input ??= new GetMyRowsInputDto();

        var skipCount = Math.Max(input.SkipCount, 0);
        var maxResultCount = input.MaxResultCount <= 0
            ? GetMyRowsInputDto.DefaultPageSize
            : input.MaxResultCount;

        maxResultCount = Math.Min(maxResultCount, GetMyRowsInputDto.MaxAllowedPageSize);

        var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("User is not authenticated.");

        var queryable = await _rowRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.UploadedByUserId == userId);

        var filter = input.Filter?.Trim();
        if (!filter.IsNullOrWhiteSpace())
        {
            if (filter.Length > 256)
            {
                filter = filter[..256];
            }

            var hasNumericFilter = decimal.TryParse(
                filter,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var numericFilter
            );

            queryable = queryable.Where(x =>
                x.ColumnA.Contains(filter)
                || x.ColumnB.Contains(filter)
                || x.ColumnC.Contains(filter)
                || (hasNumericFilter && x.NumericValue == numericFilter));
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorting = NormalizeSorting(input.Sorting);

        var entities = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(sorting)
                .Skip(skipCount)
                .Take(maxResultCount)
        );

        var items = entities.Select(entity => new ExcelDataRowDto
        {
            Id = entity.Id,
            BatchId = entity.BatchId,
            ColumnA = entity.ColumnA,
            ColumnB = entity.ColumnB,
            ColumnC = entity.ColumnC,
            NumericValue = entity.NumericValue,
            CreationTime = entity.CreationTime
        }).ToList();

        return new PagedResultDto<ExcelDataRowDto>(totalCount, items);
    }

    [Authorize(UploadFilePermissions.Dashboard.Default)]
    public async Task<List<ExcelChartItemDto>> GetMyChartAsync(CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("User is not authenticated.");

        var queryable = await _rowRepository.GetQueryableAsync();
        var limits = await GetExcelLimitsAsync();

        var result = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.UploadedByUserId == userId)
                .GroupBy(x => x.ColumnA)
                .Select(g => new ExcelChartItemDto
                {
                    Label = g.Key ?? string.Empty,
                    Value = g.Sum(x => x.NumericValue)
                })
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Label)
                .Take(limits.MaxChartItems)
        );

        return result;
    }

    private static string NormalizeSorting(string? sorting)
    {
        if (sorting.IsNullOrWhiteSpace())
            return $"{nameof(ExcelDataRow.CreationTime)} desc";

        var parts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0 || !TryMapSortingColumn(parts[0], out var mapped))
            return $"{nameof(ExcelDataRow.CreationTime)} desc";

        var dir = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
        return $"{mapped} {dir}";
    }

    private static bool TryMapSortingColumn(string column, out string mappedColumn)
    {
        switch (column)
        {
            case "columnA":
            case nameof(ExcelDataRow.ColumnA):
                mappedColumn = nameof(ExcelDataRow.ColumnA);
                return true;

            case "columnB":
            case nameof(ExcelDataRow.ColumnB):
                mappedColumn = nameof(ExcelDataRow.ColumnB);
                return true;

            case "columnC":
            case nameof(ExcelDataRow.ColumnC):
                mappedColumn = nameof(ExcelDataRow.ColumnC);
                return true;

            case "numericValue":
            case nameof(ExcelDataRow.NumericValue):
                mappedColumn = nameof(ExcelDataRow.NumericValue);
                return true;

            case "creationTime":
            case nameof(ExcelDataRow.CreationTime):
                mappedColumn = nameof(ExcelDataRow.CreationTime);
                return true;

            default:
                mappedColumn = string.Empty;
                return false;
        }
    }

    private async Task<ExcelRuntimeLimits> GetExcelLimitsAsync()
    {
        var maxUploadBytes = await GetPositiveLongSettingAsync(
            UploadFileSettings.ExcelData.MaxUploadBytes,
            UploadFileSettings.ExcelData.DefaultMaxUploadBytes
        );

        var maxAllowedRows = await GetPositiveIntSettingAsync(
            UploadFileSettings.ExcelData.MaxAllowedRows,
            UploadFileSettings.ExcelData.DefaultMaxAllowedRows
        );

        var maxAllowedSharedStrings = await GetPositiveIntSettingAsync(
            UploadFileSettings.ExcelData.MaxAllowedSharedStrings,
            UploadFileSettings.ExcelData.DefaultMaxAllowedSharedStrings
        );

        var maxChartItems = await GetPositiveIntSettingAsync(
            UploadFileSettings.ExcelData.MaxChartItems,
            UploadFileSettings.ExcelData.DefaultMaxChartItems
        );

        return new ExcelRuntimeLimits(
            maxUploadBytes,
            maxAllowedRows,
            maxAllowedSharedStrings,
            maxChartItems
        );
    }

    private async Task<long> GetPositiveLongSettingAsync(string settingName, long defaultValue)
    {
        var rawValue = await _settingProvider.GetOrNullAsync(settingName);

        return long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
               && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }

    private async Task<int> GetPositiveIntSettingAsync(string settingName, int defaultValue)
    {
        var rawValue = await _settingProvider.GetOrNullAsync(settingName);

        return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
               && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }

    private async Task<MemoryStream> EnsureSeekableAsync(
        Stream inputStream,
        long? declaredLength,
        long maxUploadBytes,
        CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream(
            capacity: declaredLength.HasValue
                      && declaredLength.Value > 0
                      && declaredLength.Value <= int.MaxValue
                ? (int)declaredLength.Value
                : 0
        );

        var buffer = new byte[81920];
        long totalRead = 0;

        while (true)
        {
            var read = await inputStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read <= 0)
            {
                break;
            }

            totalRead += read;
            if (totalRead > maxUploadBytes)
            {
                throw new UserFriendlyException(L["ExcelUpload:FileTooLarge"]);
            }

            await memoryStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private List<ParsedExcelRow> ParseExcelRows(Stream fileStream, int maxRows, int maxSharedStrings)
    {
        using var spreadsheetDocument = SpreadsheetDocument.Open(fileStream, false);

        var workbookPart = spreadsheetDocument.WorkbookPart
            ?? throw new UserFriendlyException(L["ExcelUpload:WorkbookStructureInvalid"]);

        if (workbookPart.Workbook == null)
        {
            throw new UserFriendlyException(L["ExcelUpload:WorkbookStructureInvalid"]);
        }

        var firstSheet = workbookPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault();
        if (firstSheet == null)
        {
            return new List<ParsedExcelRow>();
        }

        var relationshipId = firstSheet.Id?.Value;
        if (relationshipId.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException(L["ExcelUpload:WorkbookRelationshipsMissing"]);
        }

        WorksheetPart? worksheetPart;
        try
        {
            worksheetPart = workbookPart.GetPartById(relationshipId) as WorksheetPart;
        }
        catch
        {
            throw new UserFriendlyException(L["ExcelUpload:WorkbookRelationshipsMissing"]);
        }

        if (worksheetPart?.Worksheet == null)
        {
            return new List<ParsedExcelRow>();
        }

        var sharedStrings = LoadSharedStrings(workbookPart, maxSharedStrings);
        return ParseWorksheet(worksheetPart, sharedStrings, maxRows);
    }

    private static List<ParsedExcelRow> ParseWorksheet(
        WorksheetPart worksheetPart,
        IReadOnlyList<string> sharedStrings,
        int maxRows)
    {
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        if (sheetData == null)
        {
            return new List<ParsedExcelRow>();
        }

        var parsedRows = new List<ParsedExcelRow>();
        var skippedHeader = false;

        foreach (var row in sheetData.Elements<Row>())
        {
            var cellsByColumn = new SortedDictionary<int, string>();

            foreach (var cell in row.Elements<Cell>())
            {
                var columnIndex = GetColumnIndex(cell.CellReference?.Value ?? string.Empty);
                if (columnIndex < 0)
                {
                    continue;
                }

                var value = ReadCellValue(cell, sharedStrings).Trim();
                cellsByColumn[columnIndex] = value;
            }

            if (cellsByColumn.Count == 0)
            {
                continue;
            }

            if (!skippedHeader)
            {
                skippedHeader = true;
                continue;
            }

            var columnA = GetValueByColumn(cellsByColumn, 0);
            var columnB = GetValueByColumn(cellsByColumn, 1);
            var columnC = GetValueByColumn(cellsByColumn, 2);
            var numericValue = ParseDecimal(GetValueByColumn(cellsByColumn, 3));

            if (string.IsNullOrWhiteSpace(columnA)
                && string.IsNullOrWhiteSpace(columnB)
                && string.IsNullOrWhiteSpace(columnC)
                && numericValue == 0)
            {
                continue;
            }

            parsedRows.Add(new ParsedExcelRow(columnA, columnB, columnC, numericValue));

            if (parsedRows.Count >= maxRows)
            {
                break;
            }
        }

        return parsedRows;
    }

    private static string GetValueByColumn(IReadOnlyDictionary<int, string> cellsByColumn, int columnIndex)
        => cellsByColumn.TryGetValue(columnIndex, out var value)
            ? value?.Trim() ?? string.Empty
            : string.Empty;

    private static decimal ParseDecimal(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 0;
        }

        if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var currentCultureValue))
        {
            return currentCultureValue;
        }

        return 0;
    }

    private static int GetColumnIndex(string cellReference)
    {
        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        if (string.IsNullOrWhiteSpace(letters))
        {
            return -1;
        }

        var index = 0;
        foreach (var letter in letters)
        {
            index = (index * 26) + (char.ToUpperInvariant(letter) - 'A' + 1);
        }

        return index - 1;
    }

    private IReadOnlyList<string> LoadSharedStrings(WorkbookPart workbookPart, int maxSharedStrings)
    {
        var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
        if (sharedStringTable == null)
        {
            return Array.Empty<string>();
        }

        var values = sharedStringTable
            .Elements<SharedStringItem>()
            .Select(ReadSharedStringItem)
            .ToList();

        if (values.Count > maxSharedStrings)
        {
            throw new UserFriendlyException(L["ExcelUpload:SharedStringsTooLarge"]);
        }

        return values;
    }

    private static string ReadSharedStringItem(SharedStringItem item)
    {
        if (item.Text != null)
        {
            return item.Text.Text ?? string.Empty;
        }

        return item.InnerText ?? string.Empty;
    }

    private static string ReadCellValue(Cell cell, IReadOnlyList<string> sharedStrings)
    {
        var rawValue = cell.CellValue?.Text ?? cell.InnerText ?? string.Empty;
        var cellType = cell.DataType?.Value;

        if (cellType == null)
        {
            return rawValue;
        }

        if (cellType.Value == CellValues.SharedString)
        {
            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sharedStringIndex)
                && sharedStringIndex >= 0
                && sharedStringIndex < sharedStrings.Count)
            {
                return sharedStrings[sharedStringIndex];
            }

            return string.Empty;
        }

        if (cellType.Value == CellValues.InlineString)
        {
            return cell.InlineString?.Text?.Text
                   ?? cell.InlineString?.InnerText
                   ?? rawValue;
        }

        if (cellType.Value == CellValues.Boolean)
        {
            return rawValue == "1" ? "TRUE" : "FALSE";
        }

        return rawValue;
    }

    private sealed record ExcelRuntimeLimits(
        long MaxUploadBytes,
        int MaxAllowedRows,
        int MaxAllowedSharedStrings,
        int MaxChartItems
    );

    private sealed record ParsedExcelRow(string ColumnA, string ColumnB, string ColumnC, decimal NumericValue);
}





