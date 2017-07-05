import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'am-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent {
  constructor(private authService: AuthService) { }

  public isAuthorized(){
  	return this.authService.isAuthorized();
  }
}
