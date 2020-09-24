import { Injectable } from '@angular/core';
import { RequestHelperService } from './request-helper.service';

@Injectable()
export class TestService {
    public UrlPre = '/Controller';

    constructor(private requestHelperService: RequestHelperService) {
    }

    checkData(params): Promise<any> {
        // 这里可以调用服务，验证是否存在相同编码
        // 例子简化为前端验证
        const pRequest =new Promise(function(resolve, reject) {
            let existCodeList=['1','2','3'];
            if(existCodeList.indexOf(params.code) > -1){
                resolve(true);
            }
            resolve(false);
        }).then((ret: any) => {
            return ret;
        });

        return Promise.race([this.requestHelperService.getTimeoutPromise(), pRequest]).catch(ret => {
            this.requestHelperService.handleRequestError(ret, true);
        });
        
        // return this.requestHelperService.postRequest(`${this.UrlPre}/CheckData`, params);
    }

    callNormal(): Promise<any> {
        // resolve
        // reject
        return new Promise(function (resolve,reject){
            resolve('调用正常')
        }).then(function(value){
            return value;
        }).catch(function onRejected(error){
            return error;
        });
    }

    callException(): Promise<any> {
        return new Promise(function (resolve,reject){
            reject('调用异常')
        }).then(function(value){
            return value;
        }).catch(function onRejected(error){
            return error;
        });
    }

}
