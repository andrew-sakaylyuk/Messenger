import { RequestOptions, Headers } from '@angular/http';

export function createRequestOptions(withToken:boolean=false,
    additionalHeaders?:Headers): RequestOptions {

  let requestOptions = new RequestOptions();
  let headers = new Headers();
  headers.append("Content-Type", 'application/json');
  headers.append("Access-Control-Allow-Origin", "*");
  headers.append("Access-Control-Allow-Headers", "Origin, Authorization, Content-Type");

  if(withToken) {
    let user = JSON.parse(sessionStorage.getItem('user'));
    if(user) {
      const token = user.token;
      headers.append('Authorization', token);
      requestOptions.withCredentials = true;
    }
  }

  if(additionalHeaders) {
    additionalHeaders.forEach((values, header) => {
      values.forEach(value => {
        headers.append(header, value);
      });
    });
  }

  requestOptions.headers = headers;

  return requestOptions;
}