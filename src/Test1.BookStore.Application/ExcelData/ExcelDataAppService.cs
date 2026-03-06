using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Test1.BookStore.ExcelData;

[Authorize]
public class ExcelDataAppService : ApplicationService, IExcelDataAppService
{
    // tune these as you like
    private const long MaxUploadBytes = 10 * 1024 * 1024;          // 10 MB
    private const int MaxAllowedRows = 20_000;                     // safety cap
    private const int MaxAllowedSharedStrings = 200_000;           // safety cap
    private const int MaxChartItems = 10;

    private readonly IRepository<ExcelImportBatch, Guid> _batchRepository;
    private readonly IRepository<ExcelDataRow, Guid> _rowRepository;

    public ExcelDataAppService(
        IRepository<ExcelImportBatch, Guid> batchRepository,
        IRepository<ExcelDataRow, Guid> rowRepository)
    {
        _batchRepository = batchRepository;
        _rowRepository = rowRepository;
    }

    public async Task<ExcelUploadResultDto> UploadAsync(IRemoteStreamContent file, CancellationToken cancellationToken = default)
    {
        if (file == null)
            throw new UserFriendlyException(L["ExcelUpload:FileIsRequired"]);

        var fileName = file.FileName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException(L["ExcelUpload:OnlyXlsxAllowed"]);

        // If ContentLength is available, enforce a hard limit (best defense)
        if (file.ContentLength.HasValue && file.ContentLength.Value > MaxUploadBytes)
            throw new UserFriendlyException(L["ExcelUpload:FileTooLarge"]);

        await using var inputStream = file.GetStream();

        // Ensure we have a seekable stream for ZipArchive; copy if needed.
        await using var excelStream = await EnsureSeekableAsync(inputStream, file.ContentLength, cancellationToken);

        if (excelStream.Length == 0)
            throw new UserFriendlyException(L["ExcelUpload:FileIsEmpty"]);

        excelStream.Position = 0;

        List<ParsedExcelRow> parsedRows;
        try
        {
            parsedRows = ParseExcelRows(
                excelStream,
                maxRows: MaxAllowedRows,
                maxSharedStrings: MaxAllowedSharedStrings
            );
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch
        {
            // keep the message user-friendly; you can localize it
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

        // Build entities and bulk insert (massive perf improvement)
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

        // No need to call SaveChanges manually; ABP UoW will commit.

        return new ExcelUploadResultDto
        {
            Id = batch.Id,
            FileName = batch.FileName,
            UploadTimeUtc = batch.UploadTimeUtc,
            TotalRows = batch.TotalRows
        };
    }

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

    public async Task<List<ExcelChartItemDto>> GetMyChartAsync(CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("User is not authenticated.");

        var queryable = await _rowRepository.GetQueryableAsync();

        // Do Order/Take in query (scales better)
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
                .Take(MaxChartItems)
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

    private static async Task<MemoryStream> EnsureSeekableAsync(
        Stream inputStream,
        long? declaredLength,
        CancellationToken cancellationToken)
    {
        // If already seekable, just copy to memory only if you want to enforce MaxUploadBytes via actual stream length.
        // Here we always copy but enforce a hard cap while copying.
        var ms = new MemoryStream(capacity: declaredLength.HasValue && declaredLength.Value > 0 && declaredLength.Value <= int.MaxValue
            ? (int)declaredLength.Value
            : 0);

        var buffer = new byte[81920];
        long total = 0;

        while (true)
        {
            var read = await inputStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read <= 0) break;

            total += read;
            if (total > MaxUploadBytes)
                throw new UserFriendlyException("File is too large."); // replace with L["ExcelUpload:FileTooLarge"]

            await ms.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        ms.Position = 0;
        return ms;
    }

    private static List<ParsedExcelRow> ParseExcelRows(Stream fileStream, int maxRows, int maxSharedStrings)
    {
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: true);

        var workbookEntry = archive.GetEntry("xl/workbook.xml")
            ?? throw new UserFriendlyException("Workbook structure is invalid.");

        var workbookRelationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels")
            ?? throw new UserFriendlyException("Workbook relationships are missing.");

        var workbookDocument = XDocument.Load(workbookEntry.Open());
        var workbookNamespace = workbookDocument.Root?.Name.Namespace ?? XNamespace.None;
        var relationshipNamespace = XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var workbookRelationshipsDocument = XDocument.Load(workbookRelationshipsEntry.Open());
        var workbookRelationshipsNamespace = workbookRelationshipsDocument.Root?.Name.Namespace ?? XNamespace.None;

        var sheetPathByRelationshipId = workbookRelationshipsDocument
            .Descendants(workbookRelationshipsNamespace + "Relationship")
            .Where(r =>
                string.Equals((string?)r.Attribute("Type"),
                    "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet",
                    StringComparison.OrdinalIgnoreCase))
            .Select(r => new
            {
                Id = (string?)r.Attribute("Id"),
                Target = (string?)r.Attribute("Target")
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Target))
            .ToDictionary(x => x.Id!, x => NormalizeWorksheetPath(x.Target!));

        var firstSheetRelId = workbookDocument
            .Descendants(workbookNamespace + "sheet")
            .Select(sheet => (string?)sheet.Attribute(relationshipNamespace + "id"))
            .FirstOrDefault();

        if (firstSheetRelId.IsNullOrWhiteSpace())
            return new List<ParsedExcelRow>();

        if (!sheetPathByRelationshipId.TryGetValue(firstSheetRelId!, out var worksheetPath))
            return new List<ParsedExcelRow>();

        var worksheetEntry = archive.GetEntry(worksheetPath);
        if (worksheetEntry == null)
            return new List<ParsedExcelRow>();

        var sharedStrings = LoadSharedStrings(archive, maxSharedStrings);
        return ParseWorksheet(worksheetEntry, sharedStrings, maxRows);
    }

    private static List<ParsedExcelRow> ParseWorksheet(
        ZipArchiveEntry worksheetEntry,
        IReadOnlyList<string> sharedStrings,
        int maxRows)
    {
        var worksheetDocument = XDocument.Load(worksheetEntry.Open());
        var worksheetNamespace = worksheetDocument.Root?.Name.Namespace ?? XNamespace.None;

        var parsedRows = new List<ParsedExcelRow>();
        var skippedHeader = false;

        foreach (var rowElement in worksheetDocument.Descendants(worksheetNamespace + "row"))
        {
            var cellsByColumn = new SortedDictionary<int, string>();

            foreach (var cellElement in rowElement.Elements(worksheetNamespace + "c"))
            {
                var columnIndex = GetColumnIndex((string?)cellElement.Attribute("r") ?? string.Empty);
                if (columnIndex < 0) continue;

                var value = ReadCellValue(cellElement, worksheetNamespace, sharedStrings).Trim();
                cellsByColumn[columnIndex] = value;
            }

            if (cellsByColumn.Count == 0) continue;

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
                break;
        }

        return parsedRows;
    }

    private static string GetValueByColumn(IReadOnlyDictionary<int, string> cellsByColumn, int columnIndex)
        => cellsByColumn.TryGetValue(columnIndex, out var value) ? value?.Trim() ?? string.Empty : string.Empty;

    private static decimal ParseDecimal(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue)) return 0;

        if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
            return invariantValue;

        if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var currentCultureValue))
            return currentCultureValue;

        return 0;
    }

    private static int GetColumnIndex(string cellReference)
    {
        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        if (string.IsNullOrWhiteSpace(letters)) return -1;

        var index = 0;
        foreach (var letter in letters)
            index = (index * 26) + (char.ToUpperInvariant(letter) - 'A' + 1);

        return index - 1;
    }

    private static IReadOnlyList<string> LoadSharedStrings(ZipArchive archive, int maxSharedStrings)
    {
        var sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (sharedStringsEntry == null) return Array.Empty<string>();

        var document = XDocument.Load(sharedStringsEntry.Open());
        var ns = document.Root?.Name.Namespace ?? XNamespace.None;

        var list = document
            .Descendants(ns + "si")
            .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value)))
            .ToList();

        if (list.Count > maxSharedStrings)
            throw new UserFriendlyException("Excel file is too large (shared strings).");

        return list;
    }

    private static string ReadCellValue(XElement cellElement, XNamespace ns, IReadOnlyList<string> sharedStrings)
    {
        var type = (string?)cellElement.Attribute("t") ?? string.Empty;

        if (string.Equals(type, "s", StringComparison.OrdinalIgnoreCase))
        {
            var idxText = cellElement.Element(ns + "v")?.Value;
            if (int.TryParse(idxText, out var idx) && idx >= 0 && idx < sharedStrings.Count)
                return sharedStrings[idx];

            return string.Empty;
        }

        if (string.Equals(type, "inlineStr", StringComparison.OrdinalIgnoreCase))
            return string.Concat(cellElement.Descendants(ns + "t").Select(t => t.Value));

        if (string.Equals(type, "b", StringComparison.OrdinalIgnoreCase))
            return cellElement.Element(ns + "v")?.Value == "1" ? "TRUE" : "FALSE";

        return cellElement.Element(ns + "v")?.Value ?? string.Empty;
    }

    private static string NormalizeWorksheetPath(string target)
    {
        var t = (target ?? string.Empty).Replace('\\', '/').Trim();

        // remove leading slashes
        t = t.TrimStart('/');

        // remove leading ../ segments (common relative path pattern)
        while (t.StartsWith("../", StringComparison.OrdinalIgnoreCase))
            t = t.Substring(3);

        // if already includes xl/, keep it
        if (!t.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
            t = $"xl/{t}";

        return t;
    }

    private sealed record ParsedExcelRow(string ColumnA, string ColumnB, string ColumnC, decimal NumericValue);
}