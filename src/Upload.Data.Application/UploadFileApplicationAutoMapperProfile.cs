using AutoMapper;
using Upload.Data.ExcelData;

namespace Upload.Data;

public class UploadFileApplicationAutoMapperProfile : Profile
{
    public UploadFileApplicationAutoMapperProfile()
    {
        CreateMap<ExcelDataRow, ExcelDataRowDto>();

        CreateMap<CreateExcelImportBatchMapInput, ExcelImportBatch>()
            .ConstructUsing(source => new ExcelImportBatch(
                source.Id,
                source.TenantId,
                source.FileName,
                source.UploadedByUserId,
                source.UploadTimeUtc
            ));

        CreateMap<CreateExcelDataRowMapInput, ExcelDataRow>()
            .ConstructUsing(source => new ExcelDataRow(
                source.Id,
                source.BatchId,
                source.UploadedByUserId,
                source.ColumnA,
                source.ColumnB,
                source.ColumnC,
                source.NumericValue
            ));
    }
}
