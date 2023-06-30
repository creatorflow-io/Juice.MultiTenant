import { Component, AfterViewInit, ViewChild } from '@angular/core';
import { TenantAdminService } from './shared/services/tenant-admin.service';
import { MatMultiSort, MatMultiSortTableDataSource, TableData  } from 'ngx-mat-multi-sort';
import { MatPaginator } from '@angular/material/paginator';
import { TenantBasic } from './shared/models/tenant.model';
import { ActivatedRoute, Router, UrlSerializer } from '@angular/router';
import { Location } from '@angular/common';
import { TenantStatus, TenantStatusHelper } from './shared/models/tenant.status';
import { TableQueryResult } from './shared/models/table.query-result';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent, TenantCreateComponent, TenantDetailComponent, TenantUpdateComponent } from '../public-api';
import { DictBuilderComponent, KeyValue, ModelType } from '@juice-js/dict-builder';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { TenantSetting } from './shared/models/tenant.setting.model';
import { TenantConfiguration } from './shared/tenant-configuration';

@Component({
  selector: 'juice-tenants',
  templateUrl: './tenants.component.html',
  styleUrls: ['./tenants.component.scss']
})
export class TenantsComponent implements AfterViewInit{

  displayedColumns: string[] = ['name', 'identifier', 'status', 'actions'];
  table: TableData<TenantBasic>;
  
  tenantStatus = TenantStatus;
  statusHelper = TenantStatusHelper;
  statusOptions = Object.values(TenantStatus).filter(value => typeof value === 'string')
  .map(value => value as TenantStatus);

  get statuses(){
    return this.form.get("statuses");
  }
  
  get filterText(){
    return this.form.get("filterText");
  }

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatMultiSort) sort!: MatMultiSort;

  form: FormGroup = new FormGroup({});

  constructor(private tenantService: TenantAdminService,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    private serializer: UrlSerializer,
    private fb: FormBuilder,
    public dialog: MatDialog,
    private snackBar: MatSnackBar,
    private localizer: TranslateService,
    private options: TenantConfiguration
    ) {

      this.table = new TableData<TenantBasic>([
        { id: 'id', name: 'Id' },
        { id: 'name', name: 'Name' },
        { id: 'identifier', name: 'Identifier' },
        { id: 'status', name: 'Status' },
        { id: 'actions', name: 'Actions'}
        ]
      );
      this.table.dataSource = new MatMultiSortTableDataSource<TenantBasic>(new MatMultiSort(), false);
      
      this.table.displayedColumns = this.displayedColumns;
      
   }

  ngOnInit(): void{
    this.form = this.fb.group({
      filterText: [],
      statuses: []
    });

    let firstLoad = true;
    this.route.queryParams.subscribe(params =>{
      console.debug("route.queryParams.subscribe", params);
      if(firstLoad){
        firstLoad = false;
        this.initFormAndTableEvents();
        return;
      }
      var q = params['q'] || "";
      this.filterText?.setValue(q);
      if(params['statuses']=='any'){
        this.statuses?.setValue([]);
      }else{
        this.statuses?.setValue(params['statuses']?
         Array.isArray(params['statuses']) ? params['statuses'] : [params['statuses']]
         :[TenantStatus.Active, TenantStatus.PendingApproval, TenantStatus.PendingToActive]);
      }

      this.table.pageSize = params['pageSize']? Number.parseInt(params['pageSize']): 10;
      this.table.pageIndex = params['page']? Number.parseInt(params['page']): 0;
      var sortParams = params['sortParams'];
      var sortDirs = params['sortDirs'];
      if(sortParams && sortDirs){
          if(Array.isArray(sortParams) && Array.isArray(sortDirs)){
              this.table.sortParams = sortParams;
              this.table.sortDirs = sortDirs;
          }else if(typeof sortParams == "string" 
          && typeof (sortDirs) == "string"){
              this.table.sortParams = [sortParams];
              this.table.sortDirs = [sortDirs];
          }
      }
    });
  }

  initFormAndTableEvents(){
    this.table.nextObservable.subscribe(() => { this.getData(); });
    this.table.sortObservable.subscribe((e) => {  this.getData(); });
    this.table.previousObservable.subscribe(() => { this.getData(); });
    this.table.sizeObservable.subscribe(() => { this.getData(); });
    
    this.form.controls['statuses'].valueChanges.subscribe(value => {
      this.getData();
    });
  }

  ngAfterViewInit(): void {

    this.table.dataSource.sort = this.sort;
    
  }

  //#region filters
  clearStatuses(){
    this.statuses?.setValue([]);
  }

  clearFilterText(){
    this.filterText?.setValue("");
    this.getData();
  }

  getData(fromRoute: boolean = false) {
    var q = this.filterText?.value;
    
    if(!fromRoute){
      var statuses = this.statuses?.value;
      if(!statuses || statuses.length == 0){
        statuses = 'any';
      }
      const tree = this.router.createUrlTree([], { queryParams: { 
          q: q,
          statuses: statuses,
          page: this.table.pageIndex,
          pageSize: this.table.pageSize,
          sortParams: this.table.sortParams,
          sortDirs: this.table.sortDirs
      } });

      this.location.go(this.serializer.serialize(tree));
    }

    this.tenantService.getTenants(q, this.statuses?.value||[],
        this.table.sortParams, this.table.sortDirs, this.table.pageIndex, this.table.pageSize
        )
        .subscribe((rs : TableQueryResult<TenantBasic>)=> {
            this.table.totalElements = rs.count;
            this.table.pageIndex = rs.page-1;
            this.table.pageSize = rs.pageSize;
            this.table.data = rs.data;
        });
  }

  //#endregion

  //#region actions
  add(){
    const dialogRef = this.dialog.open(TenantCreateComponent, {
      width: this.options.dialogWidth,
      maxHeight: this.options.dialogMaxHeight,
      data: {}
    });

    let instance = dialogRef.componentInstance;
    instance.created.subscribe((id: string) => {
      this.getData();
      this.openSnackBar("Tenant was created!");
      dialogRef.close();
    });
    instance.cancelled.subscribe(() => {
      this.openSnackBar("Tenant creation was cancelled!");
      dialogRef.close();
    });
  }

  edit(id: string){
    console.debug("edit", id);
    const dialogRef = this.dialog.open(TenantUpdateComponent, {
      width: this.options.dialogWidth,
      maxHeight: this.options.dialogMaxHeight,
      data: {id: id}
    });

    let instance = dialogRef.componentInstance;
    instance.setId(id);
    instance.updated.subscribe((id: string) => {
      this.getData();
      this.openSnackBar("Tenant was updated!");
      dialogRef.close();
    });
    instance.cancelled.subscribe(() => {
      this.openSnackBar("Tenant update was cancelled!");
      dialogRef.close();
    });
  }

  detail(id: string){
    const dialogRef = this.dialog.open(TenantDetailComponent, {
      width: this.options.dialogWidth,
      maxHeight: this.options.dialogMaxHeight,
    });
    let instance = dialogRef.componentInstance;
    instance.loadTenant(id);
  }

  delete(id: string){
    console.debug("delete", id);
    this.confirm(this.doDelete.bind(this, id),
    {
      title: "Dangerous action!", 
      message: "Please confirm that you want to delete this tenant? All data will be deleted and this action cannot be undone.",
      messageClasses: "warn-color",
      buttonText:"Delete forever",
      buttonColor:"warn"
    });
  }

  doDelete(id: string){
    this.tenantService.deleteTenant(id).subscribe({
      complete: () => { 
        this.getData();
        this.openSnackBar("Tenant was deleted!");
      }, error: (error: any) => {
        this.openSnackBar("Error deleting tenant!");
        console.debug(error);
      }});
  }

  activate(id: string){
    console.debug("activate", id);
    this.confirm(this.doActivate.bind(this, id),
    {
      message: "Please confirm that you want to activate this tenant.",
      messageClasses: "success-color",
      buttonText:"Activate",
      buttonColor:"success"
    });
  }

  doActivate(id: string){
    this.tenantService.activateTenant(id).subscribe({
      complete: () => {
        this.getData();
        this.openSnackBar("Tenant was activated!");
      }, error: (error: any) => {
        this.openSnackBar("Error activating tenant!");
        console.debug(error);
      }
    });
  }

  reactivate(id: string){
    console.debug("reactivate", id);
    this.confirm(this.doReactivate.bind(this, id),
    {
      message: "Please confirm that you want to reactivate this tenant.",
      messageClasses: "success-color",
      buttonText:"Reactivate"
    });
  }

  doReactivate(id: string){
    this.tenantService.reactivateTenant(id).subscribe({
      complete: () => {
        this.getData();
        this.openSnackBar("Tenant was reactivated!");
      }, error: (error: any) => {
        this.openSnackBar("Error reactivating tenant!");
        console.debug(error);
      }
    });
  }

  deactivate(id: string){
    console.debug("deactivate", id);
    this.confirm(this.doDeactivate.bind(this, id),
    {
      message: "Please confirm that you want to deactivate this tenant. Users will not be able to login!",
      messageClasses: "warn-color",
      buttonText:"Deactivate",
      buttonColor:"warn"
    });
  }

  doDeactivate(id: string){
    this.tenantService.deactivateTenant(id).subscribe({
      complete: () => {
        this.getData();
        this.openSnackBar("Tenant was activated!");
      }, error: (error: any) => {
        this.openSnackBar("Error activating tenant!");
        console.debug(error);
      }
    });
  }

  suspend(id: string){
    console.debug("suspend", id);
    this.confirm(this.doSuspend.bind(this, id),
    {
      message: "Please confirm that you want to suspend this tenant. Users will not be able to login!",
      messageClasses: "warn-color",
      buttonText:"Suspend",
      buttonColor:"warn"
    });
  }

  doSuspend(id: string){
    this.tenantService.suspendTenant(id).subscribe({
      complete: () => {
        this.getData();
        this.openSnackBar("Tenant was suspended!");
      }, error: (error: any) => {
        this.openSnackBar("Error suspending tenant!");
        console.debug(error);
      }
    });
  }

  abandon(id: string){
    console.debug("abandon", id);
    this.confirm(this.doAbandon.bind(this, id),
    {
      message: "Please confirm that you want to abandon this tenant. All data will be deleted and this action cannot be undone.",
      messageClasses: "warn-color",
      buttonText:"Abandon",
      buttonColor:"warn"
    });
  }

  doAbandon(id: string){
    this.tenantService.abandonTenant(id).subscribe({
      complete: () => {
        this.getData();
        this.openSnackBar("Tenant was abandoned!");
      }, error: (error: any) => {
        this.openSnackBar("Error abandoning tenant!");
        console.debug(error);
      }
    });
  }

  approve(id: string){
    console.debug("approve", id);
    this.confirm(this.doApprove.bind(this, id),
    {
      message: "Please confirm that you want to approve this tenant.",
      messageClasses: "success-color",
      buttonText:"Approve",
      buttonColor:"success"
    });
  }

  doApprove(id: string){
    this.tenantService.approveTenant(id).subscribe({
      complete: () => {
        this.getData();
        this.openSnackBar("Tenant was approved!");
      }, error: (error: any) => {
        this.openSnackBar("Error approving tenant!");
        console.debug(error);
      }
    });
  }

  reject(id: string){
    console.debug("reject", id);
    this.confirm(this.doReject.bind(this, id),
    {
      message: "Please confirm that you want to reject this tenant.",
      messageClasses: "warn-color",
      buttonText:"Reject",
      buttonColor:"warn"
    });
  }

  doReject(id: string){
    this.tenantService.rejectTenant(id).subscribe({
      complete: () => {
        this.getData();
        this.openSnackBar("Tenant was rejected!");
      }, error: (error: any) => {
        this.openSnackBar("Error rejecting tenant!");
        console.debug(error);
      }
    });
  }

  settings(id: string){
    const dialogRef = this.dialog.open(DictBuilderComponent, {
      width: this.options.dialogWidth,
      maxHeight: this.options.dialogMaxHeight,
    });
    
    let instance = dialogRef.componentInstance;
    instance.loading = true;
    instance.type = ModelType.Configuration;

    this.tenantService.getTenantSettings(id).subscribe({
      next: (settings: TenantSetting[]) => {
        instance.loading = false;
        instance.models = settings.map(s => new KeyValue(s.key, s.value, s.inherited, s.overridden));
      }, error: (error: any) => {
        this.openSnackBar("Error getting tenant settings!");
        console.debug(error);
      }
    });

    instance.saved.subscribe((model: any) => {
      var settings = instance.models.map(m => new TenantSetting(m.key, m.value, m.inherited));
      this.tenantService.updateTenantSettings(id, settings).subscribe({
        next: () => {
          this.openSnackBar("Tenant's settings was updated!");
          dialogRef.close();
        }, error: (error: any) => {
          this.openSnackBar("Error updating tenant settings!");
          console.debug(error);
        }
      });
    });
    instance.cancelled.subscribe(() => {
      this.openSnackBar("Tenant setting was cancelled!");
      dialogRef.close();
    });
  }
  //#endregion

  //#region status rules
  canActivate(status: TenantStatus){
    var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
    return s == TenantStatus.New
    || s == TenantStatus.Initialized
    || s == TenantStatus.Approved
    || s == TenantStatus.PendingToActive;
  }

  canReactivate(status: TenantStatus){
    var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
    return s == TenantStatus.Inactive || s == TenantStatus.Suspended;
  }

  canDeactivate(status: TenantStatus){
    var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
    return s == TenantStatus.Active;
  }

  canSuspend(status: TenantStatus){
    var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
    return s == TenantStatus.Active || s == TenantStatus.Inactive;
  }

  canDelete(status: TenantStatus){
    var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
    return s == TenantStatus.New
    || s == TenantStatus.Initialized
    || s == TenantStatus.Approved
    || s == TenantStatus.PendingToActive
    || s == TenantStatus.Inactive
    || s == TenantStatus.Suspended
    || s == TenantStatus.Abandoned;
  }

  canAbandon(status: TenantStatus){
    var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
    return s!= TenantStatus.Abandoned;
  }

  canApproval(status: TenantStatus){
    var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
    return s == TenantStatus.PendingApproval;
  }

  //#endregion

  //#region common
  
  statusCompareFn(o1: TenantStatus, o2: TenantStatus){
    return o1 == o2 || (o1 == null && o2 == null) || (o1 == undefined && o2 == undefined)
    || (o1 == null && o2 == undefined) || (o1 == undefined && o2 == null);
  }

  confirm(callback: Function, options: any){
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: options
    });

    let instance = dialogRef.componentInstance;
    instance.confirmed.subscribe(() => {
      dialogRef.close();
      callback();
    });
    instance.cancelled.subscribe(() => {
      dialogRef.close();
    });
  }

  openSnackBar(message: string) {
    this.localizer.get([message, "Close"]).subscribe((value: any) => {
      this.snackBar.open(value[message], value["Close"], {
          duration: 3000
      });
    });
  }

  //#endregion
}
