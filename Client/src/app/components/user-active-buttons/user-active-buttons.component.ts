import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'am-user-active-buttons',
  templateUrl: './user-active-buttons.component.html',
  styleUrls: ['./user-active-buttons.component.scss'],
  providers: [ UsersService ]
})
export class UserActiveButtonsComponent implements OnInit {

  type : string;
  @Input() id: number;

  constructor(private usersService: UsersService, private router: Router) { 
	this.type = "add";
  }

  ngOnInit() {
  	this.initButton();
  }

  onSend(){
    this.router.navigate(['/chats'], { queryParams: { id: this.id } });
  }

  onAdd() {
    this.type = "disabled";
    this.usersService.addFriend(this.id).subscribe();
  }

  onConfirm() {
    this.type = "delete";
    this.usersService.confirmRequest(this.id).subscribe();
  }

  onDelete() {
    this.type = "add";
    this.usersService.deleteFriend(this.id).subscribe();
  }

  private async initButton(){
    if(await this.usersService.areFriends(this.id)) {
        this.type = "delete";
    } else if(await this.usersService.userSentRequest(this.id)){
      this.type = "confirm";
    } else if (await this.usersService.currentUserSentRequest(this.id)) { 
      this.type = "disabled";
    } else {
      this.type = "add";
    }
  }

}
