using AutoMapper;
using Upload.Data.ExcelData;

namespace Upload.Data;

public class UploadFileApplicationAutoMapperProfile : Profile
{
    public UploadFileApplicationAutoMapperProfile()
    {
        CreateMap<ExcelDataRow, ExcelDataRowDto>();
    }
}
