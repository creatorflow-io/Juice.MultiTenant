
export class TenantSetting{
    public key: string = "";
    public value: string = "";
    public inherited: boolean = false;
    public overridden: boolean = false;

    constructor(key: string, value: string, inherited: boolean = false, overridden: boolean = false){
        this.key = key;
        this.value = value;
        this.inherited = inherited;
        this.overridden = overridden;
    }
}