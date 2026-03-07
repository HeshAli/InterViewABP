using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Upload.Data.ExcelData;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Xunit;

namespace Upload.Data.EntityFrameworkCore.Applications.ExcelData;

[Collection(UploadFileTestConsts.CollectionDefinitionName)]
public class EfCoreExcelDataAppService_SmokeTests : UploadFileEntityFrameworkCoreTestBase
{
    private readonly IExcelDataAppService _excelDataAppService;

    public EfCoreExcelDataAppService_SmokeTests()
    {
        _excelDataAppService = GetRequiredService<IExcelDataAppService>();
    }

    [Fact]
    public async Task Should_Upload_Then_Query_Rows_And_Chart()
    {
        var xlsxBytes = BuildTestWorkbook();

        var uploadResult = await _excelDataAppService.UploadAsync(
            new TestRemoteStreamContent(xlsxBytes, "smoke.xlsx")
        );

        uploadResult.FileName.ShouldBe("smoke.xlsx");
        uploadResult.TotalRows.ShouldBe(2);

        var rowsResult = await _excelDataAppService.GetMyRowsAsync(new GetMyRowsInputDto
        {
            SkipCount = 0,
            MaxResultCount = 10
        });

        rowsResult.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        rowsResult.Items.Count.ShouldBeGreaterThanOrEqualTo(2);

        var uploadedRows = rowsResult.Items
            .Where(x => x.ColumnA == "GroupA" || x.ColumnA == "GroupB")
            .ToList();

        uploadedRows.Count.ShouldBe(2);
        uploadedRows.ShouldContain(x => x.ColumnA == "GroupA" && x.ColumnB == "B1" && x.ColumnC == "C1" && x.NumericValue == 10.5m);
        uploadedRows.ShouldContain(x => x.ColumnA == "GroupB" && x.ColumnB == "B2" && x.ColumnC == "C2" && x.NumericValue == 7m);

        var chartResult = await _excelDataAppService.GetMyChartAsync();

        chartResult.ShouldContain(x => x.Label == "GroupA" && x.Value == 10.5m);
        chartResult.ShouldContain(x => x.Label == "GroupB" && x.Value == 7m);
    }

    private static byte[] BuildTestWorkbook()
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(
                archive,
                "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1" />
                  </sheets>
                </workbook>
                """
            );

            WriteEntry(
                archive,
                "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml" />
                </Relationships>
                """
            );

            WriteEntry(
                archive,
                "xl/worksheets/sheet1.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData>
                    <row r="1">
                      <c r="A1" t="inlineStr"><is><t>ColumnA</t></is></c>
                      <c r="B1" t="inlineStr"><is><t>ColumnB</t></is></c>
                      <c r="C1" t="inlineStr"><is><t>ColumnC</t></is></c>
                      <c r="D1" t="inlineStr"><is><t>NumericValue</t></is></c>
                    </row>
                    <row r="2">
                      <c r="A2" t="inlineStr"><is><t>GroupA</t></is></c>
                      <c r="B2" t="inlineStr"><is><t>B1</t></is></c>
                      <c r="C2" t="inlineStr"><is><t>C1</t></is></c>
                      <c r="D2"><v>10.5</v></c>
                    </row>
                    <row r="3">
                      <c r="A3" t="inlineStr"><is><t>GroupB</t></is></c>
                      <c r="B3" t="inlineStr"><is><t>B2</t></is></c>
                      <c r="C3" t="inlineStr"><is><t>C2</t></is></c>
                      <c r="D3"><v>7</v></c>
                    </row>
                  </sheetData>
                </worksheet>
                """
            );
        }

        return ms.ToArray();
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content.Trim());
    }

    private sealed class TestRemoteStreamContent : IRemoteStreamContent
    {
        private readonly byte[] _content;

        public string FileName { get; }

        public string ContentType { get; }

        public long? ContentLength => _content.Length;

        public TestRemoteStreamContent(byte[] content, string fileName)
        {
            _content = content;
            FileName = fileName;
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        public Stream GetStream()
        {
            return new MemoryStream(_content, writable: false);
        }

        public void Dispose()
        {
        }
    }
}
