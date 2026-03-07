import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

const policies = {
  dataUploading: 'UploadFile.DataUpload.DataUploading',
  uploadedData: 'UploadFile.DataUpload.UploadedData',
  dashboard: 'UploadFile.Dashboard',
};

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: undefined,
      name: '::Menu:DataUploadSection',
      iconClass: 'fas fa-file-upload',
      layout: eLayoutType.application,
      order: 2,
    },
    {
      path: '/data-upload',
      name: '::Menu:DataUpload',
      parentName: '::Menu:DataUploadSection',
      requiredPolicy: policies.dataUploading,
      layout: eLayoutType.application,
      order: 1,
    },
    {
      path: '/data-upload/transferred-rows',
      name: '::Menu:TransferredRows',
      parentName: '::Menu:DataUploadSection',
      requiredPolicy: policies.dashboard,
      layout: eLayoutType.application,
      order: 2,
    },
    {
      path: '/data-upload/employee-uploaded-data',
      name: '::Menu:EmployeeUploadedData',
      parentName: '::Menu:DataUploadSection',
      requiredPolicy: policies.uploadedData,
      layout: eLayoutType.application,
      order: 3,
    },
  ]);
}
