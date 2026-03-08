import { CommonModule, DOCUMENT } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';
import Chart from 'chart.js/auto';
import { ExcelChartItemDto } from '../excel-data/excel-data.models';
import { ExcelDataService } from '../excel-data/excel-data.service';

@Component({
  selector: 'app-my-dashboard',
  standalone: true,
  imports: [CommonModule, LocalizationPipe],
  templateUrl: './my-dashboard.component.html',
  styleUrl: './my-dashboard.component.scss',
})
export class MyDashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly excelDataService = inject(ExcelDataService);
  private readonly localizationService = inject(LocalizationService);
  private readonly document = inject(DOCUMENT);

  @ViewChild('chartCanvas') chartCanvas?: ElementRef<HTMLCanvasElement>;

  chartItems: ExcelChartItemDto[] = [];

  private chart: Chart | null = null;

  ngOnInit(): void {
    this.document.body.classList.add('my-dashboard-fullscreen');
    this.loadChart();
  }

  ngAfterViewInit(): void {
    this.renderChart();
  }

  ngOnDestroy(): void {
    this.document.body.classList.remove('my-dashboard-fullscreen');
    this.destroyChart();
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

    const emptyLabel = this.localizationService.instant('::EmptyValue');
    const datasetLabel = this.localizationService.instant('::Chart:TotalNumericValueByColumnA');

    const labels = this.chartItems.map(item => {
      const label = item.label?.trim();
      return label ? label : emptyLabel;
    });

    const values = this.chartItems.map(item => item.value);

    this.destroyChart();

    this.chart = new Chart(this.chartCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: datasetLabel,
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
