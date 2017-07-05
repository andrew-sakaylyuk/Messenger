import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import {Router} from '@angular/router';
import { SignInRequest } from '../../../models/signin-request.model';
import {MdSnackBar} from '@angular/material';

@Component({
  selector: 'am-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss']
})
export class SigninComponent implements OnInit {

  user: SignInRequest;

  constructor(private authService: AuthService, private router: Router, 
    public snackBar: MdSnackBar) { 
    this.user = new SignInRequest();
  }

  ngOnInit() {
  }

  onSignIn() {
    this.authService.signIn(this.user)
    .subscribe(res => {
      //this.refreshToken(2000);
      this.router.navigate(['chats']);
      this.snackBar.open("You are Signed In!", 
        "Close", { duration: 3000, });
        window.location.reload();
    }, error => {
      this.snackBar.open("Invalid Username or Password!", 
        "Close", { duration: 3000, });
      console.error(error);
    } );
  }

  refreshToken(time=3000000) {
    setInterval( () => {this.authService.refreshToken();}, time); 
  }

}
