
export interface TableQueryResult<TModel>{
    page: number;
    pageSize: number;
    data: TModel[];
    count: number;
}