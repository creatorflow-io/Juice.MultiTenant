export const environment = {
    production: false,
    localize:{
        localizeApi: "https://localhost:44311/api/i18n",
        cultureApi: "https://localhost:44311/api/culture",
        appName: "testapp",
    },
    auth: {
        issuer: 'https://auth-mam.hdstation.net',
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
        apiEndpoint: "https://localhost:7079", //'https://tenants-api.hdstation.net',
        apiVersion: '2'
    }
};
