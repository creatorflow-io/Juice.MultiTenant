export class TenantCreate {
    public name: string = "";
    public identifier: string = "";
    public connectionString: string = "";
    public adminUser: string = "";
    public adminEmail: string = "";
    public properties: any = {};
}

export class TenantCreated{
    public id: string = "";
    public identifier: string = "";
}