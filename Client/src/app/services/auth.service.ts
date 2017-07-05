import { Injectable } from '@angular/core';

import {
  Http,
  Headers,
  RequestOptions,
  Response
} from '@angular/http';

import { Observable } from 'rxjs';
import { ReplaySubject } from 'rxjs/Rx';
import 'rxjs/add/operator/map'

import { environment } from '../../environments/environment';
import { SignInRequest } from '../models/signin-request.model';
import { SignUpRequest } from '../models/signup-request.model';
import { createRequestOptions } from '../utils/request-options';

@Injectable()
export class AuthService extends ReplaySubject<string> {

  private token: string;
  private username: string;
  private userId: number;
  
  private readonly apiUrl = environment.apiUrl;
  private readonly SIGNUP_URL = this.apiUrl + "api/account/signup";
  private readonly SIGNIN_URL = this.apiUrl + "api/account/signin";
  private readonly REFRESH_TOKEN_URL = this.apiUrl + "api/account/refreshToken";
  
  constructor(private http: Http) {
    super();
    if(Boolean(sessionStorage.getItem('user'))) {
      let user = JSON.parse(sessionStorage.getItem('user'));
      if(user) {
        this.saveUserDetails(user);
      }
    }
  }

  public signUp(signUpRequest: SignUpRequest) {
    let requestObj = {
      UserName: signUpRequest.username,
      Email: signUpRequest.email,
      Password: signUpRequest.password,
      ConfirmPassword: signUpRequest.confirmedPassword,
      FirstName: signUpRequest.firstName,
      LastName: signUpRequest.lastName,
      Sex: signUpRequest.gender
    };
    if(signUpRequest.birthDate != ""){
      requestObj["BirthDate"] = formatDate(signUpRequest.birthDate);
    }
    let requestParam = JSON.stringify(requestObj);
    return this.http.post(this.SIGNUP_URL,
      requestParam, createRequestOptions())
        .map((res: Response) => {
          this.saveToken(res);
          this.saveUserDetails(JSON.parse(sessionStorage.getItem('user')));
        }).catch(error => {
          throw Error(error.json() && error.json().message);
        });
  }

  public signIn(signInRequest: SignInRequest) {

    let requestParam = JSON.stringify({
      username: signInRequest.username,
      password: signInRequest.password
    });

    return this.http.post(this.SIGNIN_URL,
      requestParam, createRequestOptions())
        .map((res: Response) => {
          this.saveToken(res);
          this.saveUserDetails(JSON.parse(sessionStorage.getItem('user')));
        }).catch(error => {
          throw Error(error.json() && error.json().message);
        });
  }

  public logout() {
    this.token = null;
    this.username = null;
    this.userId = null;
    sessionStorage.removeItem('user');
  }

  public refreshToken() {
    let headers = new Headers();
    headers.append('Authorization', 'Bearer ' + 
      JSON.parse(sessionStorage.getItem('user'))["Token"] ); 
    headers.append("Content-Type", 'application/json');
    headers.append("Access-Control-Allow-Origin", "*");
    headers.append("Access-Control-Allow-Headers", "Origin, Authorization, Content-Type, Accept");
    return this.http.post(this.REFRESH_TOKEN_URL, { headers: headers })
        .map((res: Response) => {
          this.saveNewToken(res);
        });
  }

  public isAuthorized(): boolean {
    return Boolean(this.userId)
      || Boolean(this.username)
      || Boolean(this.token);
  }

  public getUsername(): string {
    return this.username;
  }

  public getUserId(): number {
    return this.userId;
  }

  public getToken(): string {
    return this.token;
  }

  private saveToken(res: Response) {
    let response;
    if (res.json() && res.json()["Token"]) {
      response = res.json();
    }
    if (Boolean(response)) {
      sessionStorage.setItem('user', JSON.stringify(response));
    } else {
      console.error(res.json());
      throw Error(res.json());
    }
  }

  private saveUserDetails(user) {
    this.token = user["Token"] || '';
    this.username = user["UserName"] || '';
    this.userId = user["Id"] || 0;
  }

  private saveNewToken(res: Response) {
    let user = JSON.parse(sessionStorage.getItem('user'));
    user.Token = res.json().Token;
    sessionStorage.setItem('user', JSON.stringify(user));
  }

}

function formatDate(date) {
    let d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();
    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;
    return [year, month, day].join('-');
}