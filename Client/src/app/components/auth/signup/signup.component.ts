import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import {Router} from '@angular/router';
import { SignUpRequest } from '../../../models/signup-request.model';
import {MdSnackBar} from '@angular/material';

@Component({
  selector: 'am-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss']
})
export class SignupComponent implements OnInit {

  user: SignUpRequest;

  constructor(private authService: AuthService, private router: Router,
    public snackBar: MdSnackBar) {
    this.user = new SignUpRequest();
  }

  ngOnInit() {
  }

  onSignUp() {
    this.authService.signUp(this.user)
    .subscribe(res => {
      this.router.navigate(['search']);
      this.snackBar.open("You are Signed Un!",
       "Close", { duration: 3000, });
    }, error => {
      console.error(error);
      this.snackBar.open("User with this Username or Email already exists!", 
      "Close", { duration: 3000, });
    });   
  }

}
