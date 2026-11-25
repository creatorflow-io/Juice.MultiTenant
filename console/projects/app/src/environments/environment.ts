export const environment = {
    production: true,
    localize:{
        localizeApi: "https://localhost:44311/api/i18n",
        cultureApi: "https://localhost:44311/api/culture",
    },
    auth: {
        issuer: 'https://host.docker.internal:44316',
        redirectUri: 'https://localhost:4200/auth/login-completed',
        postLogoutRedirectUri: 'https://localhost:4200/auth/logout-completed',
        clientId: 'tenants_admin',
        responseType: 'code',
        scope: 'openid profile roles offline_access localize_api',
        basePath : 'https://localhost:4200/auth',
    },
    layout:{
        brand: "cfio",
    },
    tenantOptions:{
        apiEndpoint: 'https://localhost:44314',
        apiVersion: '2'
    }
};
