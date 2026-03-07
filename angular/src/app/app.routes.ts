import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

const policies = {
  dataUploading: 'UploadFile.DataUpload.DataUploading',
  uploadedData: 'UploadFile.DataUpload.UploadedData',
  dashboard: 'UploadFile.Dashboard',
};

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
    canActivate: [authGuard],
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
    canActivate: [authGuard],
  },
  {
    path: 'tenant-management',
    loadChildren: () => import('@abp/ng.tenant-management').then(c => c.createRoutes()),
    canActivate: [authGuard],
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
    canActivate: [authGuard],
  },
  {
    path: 'data-upload',
    loadComponent: () => import('./data-upload/data-upload.component').then(c => c.DataUploadComponent),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: policies.dataUploading },
  },
  {
    path: 'data-upload/transferred-rows',
    loadComponent: () => import('./my-dashboard/my-dashboard.component').then(c => c.MyDashboardComponent),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: policies.dashboard },
  },
  {
    path: 'data-upload/employee-uploaded-data',
    loadComponent: () =>
      import('./employee-uploaded-data/employee-uploaded-data.component').then(c => c.EmployeeUploadedDataComponent),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: policies.uploadedData },
  },
  {
    path: 'my-dashboard',
    redirectTo: 'data-upload/transferred-rows',
    pathMatch: 'full',
  },
];