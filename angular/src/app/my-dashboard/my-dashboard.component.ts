import { CommonModule, DatePipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ListService, LocalizationPipe, PagedResultDto, type PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { NgxDatatableDefaultDirective, NgxDatatableListDirective } from '@abp/ng.theme.shared';
import Chart from 'chart.js/auto';
import { ExcelChartItemDto, ExcelDataRowDto, ExcelRowsQueryDto } from '../excel-data/excel-data.models';
import { ExcelDataService } from '../excel-data/excel-data.service';

@Component({
  selector: 'app-my-dashboard',
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
  templateUrl: './my-dashboard.component.html',
})
export class MyDashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly excelDataService = inject(ExcelDataService);
  readonly list = inject(ListService);
  readonly pageSizeOptions = [3, 5, 10, 20];

  @ViewChild('chartCanvas') chartCanvas?: ElementRef<HTMLCanvasElement>;

  rows = { items: [], totalCount: 0 } as PagedResultDto<ExcelDataRowDto>;
  chartItems: ExcelChartItemDto[] = [];

  private chart: Chart | null = null;

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

    this.loadChart();
  }

  ngAfterViewInit(): void {
    this.renderChart();
  }

  ngOnDestroy(): void {
    this.destroyChart();
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

  private loadChart(): void {
    this.excelDataService.getMyChart().subscribe(items => {
      this.chartItems = items;
      this.renderChart();
    });
  }

  private renderChart(): void {
    if (!this.chartCanvas) {
      return;
    }

    const labels = this.chartItems.map(item => item.label || '(empty)');
    const values = this.chartItems.map(item => item.value);

    this.destroyChart();

    this.chart = new Chart(this.chartCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Total numeric value by Column A',
            data: values,
            borderWidth: 1,
            borderColor: '#2b8a3e',
            backgroundColor: '#74c69d',
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
          },
        },
        plugins: {
          legend: {
            display: true,
          },
        },
      },
    });
  }

  private destroyChart(): void {
    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }
  }
}
