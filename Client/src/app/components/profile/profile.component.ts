import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router'
import  { ChangeAccountService } from '../../services/change-account.service';
import  { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { MdSnackBar, MdDialog, MdDialogRef, MdSelectModule } from '@angular/material';

@Component({
  selector: 'am-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
  providers: [ChangeAccountService]
})

export class ProfileComponent implements OnInit {

  user: User;

  constructor(private changeAccountService: ChangeAccountService, 
    private authService: AuthService, private router: Router,
    public snackBar: MdSnackBar, public dialog: MdDialog) { 
	  this.user = new User();
	  let user = JSON.parse(sessionStorage.getItem('user')); 
	  this.user.username = user.UserName;
    this.user.firstName = user.FirstName;
    this.user.lastName = user.LastName;
    this.user.email = user.Email;
    this.user.gender = user.Sex;
    if(user.BirthDate != "0001-01-01") {
      this.user.birthDate = user.BirthDate;//new Date(user.BirthDate);
    } 
  }

  ngOnInit() {
  }

  onDeleteAccount(){
    let dialogRef = this.dialog.open(ConfirmDeleteAccountDialog);
    dialogRef.afterClosed().subscribe( result => {
      if (result === true) {
        this.changeAccountService.deleteUser().subscribe( res => {
          this.snackBar.open("Your Account was deleted!", 
            "Close", { duration: 10000, });
          this.authService.logout();
          this.router.navigate(['']);
        }); 
      }
    });
  }

  onChangeUserInfo() {
    this.changeAccountService.changeUserInfo(this.user)
    .subscribe( res => {
      this.snackBar.open("UserInfo was changed successfully!", 
      "Close", {duration: 3000, });
    }, error => {
      this.snackBar.open("User with this Email already exists!", 
      "Close", { duration: 3000, });
    });
  }

  onChangeUserName(){
  	this.changeAccountService.changeUsername(this.user.username)
    .subscribe( res => {
      this.snackBar.open("Your UserName was changed successfully!", 
      "Close", {duration: 3000, });
    }, error => {
      this.snackBar.open("User with this Username already exists!", 
      "Close", { duration: 3000, });
    });
  }

  onChangePassword(oldPassword: string, 
    newPassword: string, confirmedPassword: string){
    this.changeAccountService.changePassword(oldPassword, 
      newPassword, confirmedPassword)
    .subscribe(res => {
        this.snackBar.open("Your Password was changed successfully!", 
          "Close", { duration: 3000, });
    }, error => {
      this.snackBar.open("Invalid Password!", 
      "Close", { duration: 3000, });
    });
  }

}

@Component({
  selector: 'confirm-delete-account-dialog',
  template: ` 
  <p>Are you sure you want to delete your account?</p>
  <div id="buttons">
  <button type="button" md-button color="primary"
    (click)="dialogRef.close(true)">Yes</button>
  <button type="button" md-raised-button color="primary"
    (click)="dialogRef.close(false)">No</button>
  </div>`,
  styles: ['button{ margin: auto;} #buttons{display: flex; align-content: center;}']
})
export class ConfirmDeleteAccountDialog {
  constructor(public dialogRef: MdDialogRef<ConfirmDeleteAccountDialog>) {}
}