import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

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
      path: '/data-upload',
      name: '::Menu:DataUpload',
      iconClass: 'fas fa-file-upload',
      layout: eLayoutType.application,
      order: 2,
    },
    {
      path: '/my-dashboard',
      name: '::Menu:MyDashboard',
      iconClass: 'fas fa-chart-bar',
      layout: eLayoutType.application,
      order: 3,
    },
  ]);
}
