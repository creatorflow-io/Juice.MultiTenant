export const environment = {
    production: false,
    localize:{
        localizeApi: "https://localhost:44311/api/i18n",
        cultureApi: "https://localhost:44311/api/culture",
        appName: "testapp",
    },
    auth: {
        issuer: 'https://host.docker.internal:44316',
        redirectUri: 'https://localhost:4200/auth/login-completed',
        postLogoutRedirectUri: 'https://localhost:4200/auth/logout-completed',
        clientId: 'tenants_admin',
        responseType: 'code',
        scope: 'openid profile roles tenants-api',
        basePath : 'https://localhost:4200/auth',
    },
    layout:{
        brand: "cfio",
        defaultMenuOpen: true,
        userImageUrl: "https://i.pravatar.cc/64"
    },
    tenantOptions:{
        apiEndpoint: 'https://localhost:44314',
        apiVersion: '2'
    }
};
