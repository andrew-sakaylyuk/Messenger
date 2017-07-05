import { Injectable } from '@angular/core';
import { RequestOptions, Response } from '@angular/http';
import { HttpClient } from './http-client';

import { Observable } from 'rxjs';
import { ReplaySubject } from 'rxjs/Rx';
import 'rxjs/add/operator/map'

import { environment } from '../../environments/environment';
import { createRequestOptions } from '../utils/request-options';
import { User } from '../models/user.model';

@Injectable()
export class ChangeAccountService {
  
  private readonly apiUrl = environment.apiUrl;
  private readonly CHANGE_USER_URL = this.apiUrl + "api/users";
  private readonly UPLOAD_AVATAR_URL = this.apiUrl + "api/users/UploadAvatar";
  private readonly CHANGE_USERNAME_URL = this.apiUrl + "api/users/ChangeUsername";
  private readonly CHANGE_PASSWORD_URL = this.apiUrl + "api/users/ChangePassword";
  
  constructor(private http: HttpClient) {
  }

  public deleteUser() {
    return this.http.delete(this.CHANGE_USER_URL).catch(error => {
      console.log(error);
      throw Error(error);
    });
  }

  public changeUserInfo(user: User) {
    let requestObj = {
      FirstName: user.firstName,
      LastName: user.lastName,
      Email: user.email,
      Sex: user.gender
    };
    if(user.birthDate != ""){
      requestObj["BirthDate"] = formatDate(user.birthDate);
    }
    let requestParam = JSON.stringify(requestObj);
    return this.http.put(this.CHANGE_USER_URL, requestParam)
      .map((res: Response) => {
        this.saveNewUserInfo(res);
      }).catch(error => {
        console.log(error);
        throw Error(error);
      });
  }

  public changePassword(oldPassword: string, 
    newPassword: string, confirmedPassword: string) {
    return this.http.put(this.CHANGE_PASSWORD_URL, {OldPassword: oldPassword, 
        NewPassword: newPassword, ConfirmPassword: confirmedPassword})
        .map((res: Response) => {
          this.saveNewToken(res);
        }).catch(error => {
          console.log(error);
          throw Error(error);
        });
  }

  public changeUsername(username: string){
    return this.http.put(this.CHANGE_USERNAME_URL, {UserName: username})
        .map((res: Response) => {
          this.saveNewUsername(res);
        }).catch(error => {
          console.log(error);
          throw Error(error);
        });
  }

  private saveNewUsername(res: Response) {
    let user = JSON.parse(sessionStorage.getItem('user'));
    user.UserName = res.json().UserName;
    user.Token = res.json().Token;
    sessionStorage.setItem('user', JSON.stringify(user));
  }

  private saveNewToken(res: Response) {
    let user = JSON.parse(sessionStorage.getItem('user'));
    user.Token = res.json().Token;
    sessionStorage.setItem('user', JSON.stringify(user));
  }

  private saveNewUserInfo(res: Response){
    let user = JSON.parse(sessionStorage.getItem('user'));
    user.FirstName = res.json().FirstName;
    user.LastName = res.json().LastName;
    user.Email = res.json().Email;
    user.Sex = res.json().Sex;
    user.BirthDate = res.json().BirthDate;
    sessionStorage.setItem('user', JSON.stringify(user));
  }
  
  private logout() {
    sessionStorage.removeItem('user');
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