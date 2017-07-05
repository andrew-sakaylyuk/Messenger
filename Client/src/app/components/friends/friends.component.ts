import { Component, OnInit } from '@angular/core';
import { UsersListComponent } from '../users-list/users-list.component';

@Component({
  selector: 'am-friends',
  templateUrl: './friends.component.html',
  styleUrls: ['./friends.component.scss']
})
export class FriendsComponent implements OnInit {

  public friendsCount: number;
  public requestsCount: number;

  constructor() { 
  	this.friendsCount = 0;
  	this.requestsCount =0;  
  }

  ngOnInit() { }

  public countFriends(count: number):void {
  	this.friendsCount = count;
  }

  public countRequests(count: number):void {
  	this.requestsCount = count;
  }

}
