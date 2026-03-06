import { Injectable } from '@angular/core';
import { RestService, type PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { ExcelChartItemDto, ExcelDataRowDto, ExcelRowsQueryDto, ExcelUploadResultDto } from './excel-data.models';

@Injectable({
  providedIn: 'root',
})
export class ExcelDataService {
  apiName = 'Default';

  constructor(private readonly restService: RestService) {}

  upload = (file: File): Observable<ExcelUploadResultDto> => {
    const body = new FormData();
    body.append('file', file, file.name);

    return this.restService.request<any, ExcelUploadResultDto>(
      {
        method: 'POST',
        url: '/api/app/excel-data/upload',
        body,
      },
      { apiName: this.apiName }
    );
  };

  getMyRows = (input: ExcelRowsQueryDto): Observable<PagedResultDto<ExcelDataRowDto>> =>
    this.restService.request<any, PagedResultDto<ExcelDataRowDto>>(
      {
        method: 'GET',
        url: '/api/app/excel-data/my-rows',
        params: {
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName }
    );

  getMyChart = (): Observable<ExcelChartItemDto[]> =>
    this.restService.request<any, ExcelChartItemDto[]>(
      {
        method: 'GET',
        url: '/api/app/excel-data/my-chart',
      },
      { apiName: this.apiName }
    );
}

