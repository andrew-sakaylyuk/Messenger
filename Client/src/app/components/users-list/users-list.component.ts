import {
  Component,
  Input,
  Output,
  OnInit,
  EventEmitter
} from '@angular/core';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { UsersService } from '../../services/users.service';
import  { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';

@Component({
  selector: 'am-users-list',
  templateUrl: 'users-list.component.html',
  styleUrls: ['users-list.component.scss'],
  providers: [ UsersService ]
})
export class UsersListComponent implements OnInit {

  @Input() type: string;
  @Input() searchModel: User;
  @Input() userId: number;
  @Output() onLoad: EventEmitter<number> = new EventEmitter<number>();
  public users: Array<any>;
  public currentPage: number;
  public maxPages: number;

  constructor(
    private usersService: UsersService, private authService: AuthService
  ) {
    this.type = "search";
    this.users = [];
    this.currentPage = 1;
    this.maxPages = 1;
  }

  ngOnInit() {
    this.getUsers();  
  }

  onLoadMore() {
    this.currentPage++;
    this.getUsers(this.currentPage);
  }

  getUsers(page=1){
    switch(this.type) {
      case "search": {
        this.usersService.searchUsers(this.searchModel, page)
          .subscribe(search => {
            let usersList = search["First"];
            usersList = usersList.filter(
              user => user.Id != this.authService.getUserId());
            this.maxPages = search["Second"];
            this.users = this.users.concat(usersList);
            this.onLoad.emit(this.users.length);
          }, error => {
            console.error(error);
          });
        break;
      }
      case "friends": {
        this.usersService.getFriends(page)
          .subscribe(friends => {
            this.users = this.users.concat(friends["First"]);
            this.maxPages = friends["Second"];
            this.onLoad.emit(this.users.length);
          }, error => {
            console.error(error);
          });
          break;
      }
      case "requests": {
        this.usersService.getRequests(page)
          .subscribe(requests => {
            this.users = this.users.concat(requests["First"]);
            this.maxPages = requests["Second"];
            this.onLoad.emit(this.users.length);
          }, error => {
            console.error(error);
          });
          break;
      }
      case "mutual": {
        this.usersService.getMutualFriends(this.userId, page)
          .subscribe(mutual => {
            this.users = this.users.concat(mutual["First"]);
            this.maxPages = mutual["Second"];
            this.onLoad.emit(this.users.length);
          }, error => {
            console.error(error);
          });
          break;
      }
      case "path": {
        this.usersService.getShortestPath(this.userId)
          .subscribe(path => {
            if(path[0]["UserName"] != "No Such User"){
              path.splice(0,1);
              path.splice(path.length-1,1);
              this.users = path;
              this.onLoad.emit(this.users.length);
            } 
          }, error => {
            console.error(error);
          });
          break;
      }
    }
  }

}