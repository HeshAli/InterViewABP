import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';
import { ExcelDataService } from '../excel-data/excel-data.service';
import { ExcelUploadResultDto } from '../excel-data/excel-data.models';

@Component({
  selector: 'app-data-upload',
  standalone: true,
  imports: [CommonModule, LocalizationPipe],
  templateUrl: './data-upload.component.html',
})
export class DataUploadComponent {
  selectedFile: File | null = null;
  uploadResult: ExcelUploadResultDto | null = null;
  uploadError = '';
  isUploading = false;

  constructor(
    private readonly excelDataService: ExcelDataService,
    private readonly localizationService: LocalizationService,
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files && input.files.length > 0 ? input.files[0] : null;
    this.uploadResult = null;
    this.uploadError = '';
  }

  upload(): void {
    if (!this.selectedFile || this.isUploading) {
      return;
    }

    this.isUploading = true;
    this.uploadResult = null;
    this.uploadError = '';

    this.excelDataService.upload(this.selectedFile).subscribe({
      next: result => {
        this.uploadResult = result;
        this.isUploading = false;
      },
      error: error => {
        this.uploadError =
          error?.error?.error?.message ||
          error?.error?.message ||
          error?.message ||
          this.localizationService.instant('::UploadFailed');
        this.isUploading = false;
      },
    });
  }
}
