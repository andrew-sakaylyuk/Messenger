import { Injectable } from '@angular/core';
import { RequestOptions, Response} from '@angular/http';
import { HttpClient } from './http-client';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { environment } from '../../environments/environment';
import { createRequestOptions } from '../utils/request-options';

@Injectable()
export class UploadService {

  private readonly apiUrl = environment.apiUrl;
  private readonly UPLOAD_AVATAR_URL = this.apiUrl + "api/users/UploadAvatar";

  constructor( private http: HttpClient ) {}

  public uploadFile(file: File) {
    let formData:FormData = new FormData();
    formData.append(file.name, file);
    return this.http.postImage(this.UPLOAD_AVATAR_URL, formData)
    .map(res => this.saveNewAvatarUrl(res))
    .catch(error => Observable.throw(error));
  }

  private saveNewAvatarUrl(res: Response) {
    let user = JSON.parse(sessionStorage.getItem('user'));
    user.AvatarUrl = res.json().AvatarUrl;
    sessionStorage.setItem('user', JSON.stringify(user));
  }

}