import {Injectable} from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';

@Injectable()
export class HttpClient {

  constructor(private http: Http) {}

  private createAuthorizationHeader(headers: Headers) {
    headers.append('Authorization', 'Bearer ' + 
      JSON.parse(sessionStorage.getItem('user'))["Token"] ); 
    headers.append("Content-Type", 'application/json');
    headers.append("Access-Control-Allow-Origin", "*");
    headers.append("Access-Control-Allow-Headers", "Origin, Authorization, Content-Type, Accept");
  }

  public get(url) {
    let headers = new Headers();
    this.createAuthorizationHeader(headers);
    return this.http.get(url, { headers: headers });
  }

  public post(url, data) {
    let headers = new Headers();
    this.createAuthorizationHeader(headers);
    return this.http.post(url, data, { headers: headers });
  }

  public postWithoutBody(url) {
    let headers = new Headers();
    this.createAuthorizationHeader(headers);
    return this.http.post(url, '', { headers: headers });
  }

  public postImage(url, data) {
    let headers = new Headers();
    this.createAuthorizationHeader(headers);
    headers.delete('Content-Type');
    let options = new RequestOptions({ headers: headers });
    return this.http.post(url, data, options);
  }

  public delete(url) {
    let headers = new Headers();
    this.createAuthorizationHeader(headers);
    return this.http.delete(url, { headers: headers });
  }

  public put(url, data) {
    let headers = new Headers();
    this.createAuthorizationHeader(headers);
    return this.http.put(url, data, { headers: headers });
  }

  public putWithoutBody(url) {
    let headers = new Headers();
    this.createAuthorizationHeader(headers);
    return this.http.put(url, '', { headers: headers });
  }

}