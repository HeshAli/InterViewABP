export interface ExcelUploadResultDto {
  id: string;
  fileName: string;
  uploadTimeUtc: string;
  totalRows: number;
}

export interface ExcelDataRowDto {
  id: string;
  batchId: string;
  columnA: string;
  columnB: string;
  columnC: string;
  numericValue: number;
  creationTime: string;
}

export interface ExcelChartItemDto {
  label: string;
  value: number;
}
export interface ExcelRowsQueryDto {
  sorting?: string;
  skipCount: number;
  maxResultCount: number;
}

