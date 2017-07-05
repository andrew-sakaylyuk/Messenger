import { Component, OnInit, Inject, NgZone } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'am-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {

  public newMessagesCount: Number;

  constructor(private authService: AuthService, @Inject("ChatService") public _chatService, private _ngZone: NgZone) {
  }

  ngOnInit() {
    this._chatService.startConnection();
    this._chatService.getNewMessagesCount();
    this._chatService.userOnline();
  }

  logout() {
    this.authService.logout();
    this._chatService.userOffline();
    this._chatService.stopConnection();
  }

}
