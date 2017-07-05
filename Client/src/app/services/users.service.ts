import { Injectable } from '@angular/core';

import { RequestOptions, Response } from '@angular/http';
import { HttpClient } from './http-client';

import { Observable } from 'rxjs';
import { ReplaySubject } from 'rxjs/Rx';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/toPromise';

import { environment } from '../../environments/environment';
import { createRequestOptions } from '../utils/request-options';
import { AuthService } from './auth.service';
import { User } from '../models/user.model';

@Injectable()
export class UsersService {

  constructor(private http: HttpClient, private authService: AuthService) {}

  private static readonly USER_URL = environment.apiUrl + 'api/users/';
  private static readonly REQUESTS_URL = environment.apiUrl + 'api/friends/requests?p=';
  private static readonly FRIENDS_URL = environment.apiUrl + 'api/friends?p=';
  private static readonly MUTUAL_FRIENDS_URL = environment.apiUrl + 'api/friends/mutual?userId=';
  private static readonly SHORTEST_PATH_URL = environment.apiUrl + 'api/friends/shortestPath?userId=';
  private static readonly ADD_FRIEND_URL = environment.apiUrl + 'api/friends/add/';
  private static readonly CONFIRM_FRIEND_URL = environment.apiUrl + 'api/friends/confirm/';
  private static readonly DELETE_FRIEND_URL = environment.apiUrl + 'api/friends/';
  private static readonly ARE_FRIENDS_URL = environment.apiUrl + 'api/friends/areFriends?userId=';
  private static readonly REQUEST_EXISTS_URL = environment.apiUrl + 'api/friends/friendshipRequestAlreadyExists?senderId=';

  getUser(id: number){
    return this.http.get(UsersService.USER_URL + id)
      .map(res => {
        return res.json();
      })
      .catch(error => {
        throw Error(error);
      });
  }

  getFriends(page=1): Observable<any> {
    return this.http.get(UsersService.FRIENDS_URL + page)
      .map(res => {
        return res.json();
      })
      .catch(error => {
        throw Error(error);
      });
  }

  getRequests(page=1): Observable<any> {
    return this.http.get(UsersService.REQUESTS_URL + page)
      .map(res => {
        return res.json();
      })
      .catch(error => {
        throw Error(error);
      });
  }

  getMutualFriends(userId : number, page=1): Observable<any> {
    return this.http.get(UsersService.MUTUAL_FRIENDS_URL + userId + "&p=" + page)
      .map(res => {
        return res.json();
      })
      .catch(error => {
        throw Error(error);
      });
  }

  getShortestPath(userId : number): Observable<any> {
    return this.http.get(UsersService.SHORTEST_PATH_URL + userId)
      .map(res => {
        return res.json();
      })
      .catch(error => {
        throw Error(error);
      });
  }

  searchUsers(searchModel : User, page=1): Observable<any> {
    let searchParameters = "?";
    if(searchModel.username != "") 
      searchParameters += "UserName=" + searchModel.username + "&";
    if(searchModel.firstName != "") 
      searchParameters += "FirstName=" + searchModel.firstName + "&";
    if(searchModel.lastName != "") 
      searchParameters += "LastName=" + searchModel.lastName + "&";
    if(searchModel.email != "") 
      searchParameters += "Email=" + searchModel.email + "&";
    if(searchModel.gender != "") 
      searchParameters += "Sex=" + searchModel.gender + "&";
    if(searchModel.birthDate != "") 
      searchParameters += "BirthDate=" + searchModel.birthDate + "&";
    return this.http.get(UsersService.USER_URL + searchParameters + "p=" + page)
      .map(res => {
        return res.json();
      })
      .catch(error => {
        throw Error(error);
      });
  }

  addFriend(id: number){
     return this.http.postWithoutBody(UsersService.ADD_FRIEND_URL + id)
      .catch(error => {
        throw Error(error);
      });
  }

  confirmRequest(id: number){
     return this.http.putWithoutBody(UsersService.CONFIRM_FRIEND_URL + id)
      .catch(error => {
        throw Error(error);
      });
  }

  deleteFriend(id: number){
     return this.http.delete(UsersService.DELETE_FRIEND_URL + id)
      .catch(error => {
        throw Error(error);
      });
  }

  areFriends(id: number) : Promise<boolean> {
     return this.http.get(UsersService.ARE_FRIENDS_URL + 
      this.authService.getUserId() + '&friendId='+ id)
      .map(res => res.json())
      .toPromise()
      .catch(error => {
        throw Error(error);
      });
  }

  currentUserSentRequest(id: number) : Promise<boolean>{
     return this.http.get(UsersService.REQUEST_EXISTS_URL + 
      this.authService.getUserId() + '&receiverId='+ id)
      .map(res => res.json())
      .toPromise()
      .catch(error => {
        throw Error(error);
      });
  }

  userSentRequest(id: number) : Promise<boolean>{
     return this.http.get(UsersService.REQUEST_EXISTS_URL + id
        + '&receiverId='+ this.authService.getUserId())
      .map(res => res.json())
      .toPromise()
      .catch(error => {
        throw Error(error);
      });
  }

  
}