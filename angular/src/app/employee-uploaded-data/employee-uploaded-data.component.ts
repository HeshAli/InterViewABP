import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import {
  ListService,
  LocalizationPipe,
  PagedResultDto,
  type PagedAndSortedResultRequestDto,
} from '@abp/ng.core';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { NgxDatatableDefaultDirective, NgxDatatableListDirective } from '@abp/ng.theme.shared';
import { ExcelDataRowDto, ExcelRowsQueryDto } from '../excel-data/excel-data.models';
import { ExcelDataService } from '../excel-data/excel-data.service';

@Component({
  selector: 'app-employee-uploaded-data',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    LocalizationPipe,
    NgxDatatableModule,
    NgxDatatableListDirective,
    NgxDatatableDefaultDirective,
  ],
  providers: [ListService],
  templateUrl: './employee-uploaded-data.component.html',
})
export class EmployeeUploadedDataComponent implements OnInit {
  private readonly excelDataService = inject(ExcelDataService);
  readonly list = inject(ListService);
  readonly pageSizeOptions = [3, 5, 10, 20];

  rows = { items: [], totalCount: 0 } as PagedResultDto<ExcelDataRowDto>;

  ngOnInit(): void {
    this.list.maxResultCount = 3;

    const rowsStreamCreator = (query: PagedAndSortedResultRequestDto) => {
      const request: ExcelRowsQueryDto = {
        sorting: query.sorting,
        skipCount: query.skipCount ?? 0,
        maxResultCount: query.maxResultCount ?? this.list.maxResultCount,
      };

      return this.excelDataService.getMyRows(request);
    };

    this.list.hookToQuery(rowsStreamCreator).subscribe(response => {
      this.rows = response;
    });
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement | null;
    if (!target) {
      return;
    }

    const pageSize = Number(target.value);
    if (!Number.isFinite(pageSize) || pageSize <= 0 || pageSize === this.list.maxResultCount) {
      return;
    }

    this.list.page = 0;
    this.list.maxResultCount = pageSize;
  }
}
