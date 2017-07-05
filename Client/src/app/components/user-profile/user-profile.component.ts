import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UsersService } from '../../services/users.service';
import { User } from '../../models/user.model';

@Component({
  selector: 'am-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss'],
  providers: [ UsersService ]
})
export class UserProfileComponent implements OnInit {

  id: number;
  user : User;
  avatarUrl: string;
  online: boolean;
  
  constructor(private route: ActivatedRoute, 
  	private router: Router, private usersService: UsersService) { 
    this.avatarUrl = "assets/noavatar.png";
    this.user = new User();
    this.online = false;
  }

  ngOnInit() {
  	this.route.params.subscribe(params => {
      this.id = params['id']; 
      this.resetComponentState();
    });
  }

  resetComponentState(){
    this.usersService.getUser(this.id)
    .subscribe(res => {
      this.avatarUrl = res.AvatarUrl;
      this.user.username = res.UserName;
      this.user.firstName = res.FirstName;
      this.user.lastName = res.LastName;
      this.user.email = res.Email;
      this.user.birthDate = res.BirthDate;
      this.user.gender = res.Sex;
      this.online = res.Online;
    }, error => {
      console.error(error);
    });
  }
  
}
