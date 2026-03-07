import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44316/',
  redirectUri: baseUrl,
  clientId: 'UploadFile_App',
  responseType: 'code',
  scope: 'offline_access UploadFile',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'Upload.Data',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44316',
      rootNamespace: 'Upload.Data',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;


