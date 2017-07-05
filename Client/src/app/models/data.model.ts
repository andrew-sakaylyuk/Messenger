export class Data <Data> {
    public readonly data: Data;
    public readonly pageCount: Number;
    constructor(data:Data,pageCount:Number ){
        this.data=data;
        this.pageCount=pageCount;
    }
}