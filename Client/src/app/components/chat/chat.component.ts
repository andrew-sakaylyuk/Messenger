import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ViewEncapsulation,
  Inject,
  ElementRef,
  NgZone
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PerfectScrollbarComponent } from "ngx-perfect-scrollbar";
import { fadeInAnimation } from "../../route.animation";
import { Conversation } from '../../models/conversation.model';
import { Data } from '../../models/data.model';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'am-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss'],
  encapsulation: ViewEncapsulation.None,
  host: {
    "[@fadeInAnimation]": 'true'
  },
  providers: [UsersService],
  animations: [fadeInAnimation]
})
export class ChatComponent implements OnInit, OnDestroy {

  activeConversation: Conversation;
  id: number;
  conversations: Data<Array<Conversation>>;
  conversationsSubscription: any;
  noConversations: Boolean;

  @ViewChild('chatScroll') private chatScroll: PerfectScrollbarComponent;

  constructor( @Inject('ChatService') private chatService,
    private _ngZone: NgZone, private route: ActivatedRoute,
    private router: Router,
    private usersService: UsersService, ) {
    this.conversations = new Data<Array<Conversation>>([], 0);
    //+ -> parse from string to number
    this.id = +this.chatService.getQueryString('id', window.location.href);
  }

  ngOnInit() {
    this.resetComponentState();
  }

  ngOnDestroy() {
    this.conversationsSubscription.unsubscribe();
  }

  resetComponentState() {
    this.conversationsSubscription =
      this.chatService.getConversationsEvent
      .subscribe((data: Data<Array<Conversation>>) => {
        this._ngZone.run(() => {
          this.conversations = data;
          for(let i = 0; i < this.conversations.data.length; i++) {
            this.conversations.data[i].DateTime = formatDateTime(
              new Date(this.conversations.data[i].DateTime +' UTC'));
          }
          if (this.id != 0) {
            let id = this.id;
            let conversation = this.conversations.data
              .filter(function (obj) {
                return obj.OtherUserId == id;
              });
            if (conversation.length > 0) {
              this.onActiveConversation(conversation[0]);
              this.chatService.getCorrespondence(this.id);
            }
            else {
              this.createConversation(this.id);
            }
          } else {
            //pick first one
            if (this.conversations.data.length > 0) {
              this.noConversations = false;
              this.router.navigate(['/chats'], {
                queryParams:
                { id: this.conversations
                  .data[this.conversations.data.length - 1].OtherUserId }
              });
              this.onActiveConversation(this.conversations
                .data[this.conversations.data.length - 1]);
              this.chatService.getCorrespondence(this.conversations
                .data[this.conversations.data.length - 1].OtherUserId);
            } else {
              this.noConversations = true;
            }
          }
        });
      });
    this.chatService.getNewMessagesCount();
    this.chatService.getConversations();
  }

  getConversations() {
    this.chatService.getConversations();
  }

  onActiveConversation(conversation) {
    this.activeConversation = conversation;
  }

  createConversation(senderId: number) {
      let id = this.id;
      let conversation = this.conversations.data
        .filter(function (obj) {
          return obj.OtherUserId == id;
        });
      if (conversation.length == 0) {
        this.usersService.getUser(senderId).subscribe(
          user => {
            this.activeConversation =
              new Conversation('', user.FirstName, user.LastName, 
                '', user.Id, 0, user.AvatarUrl, user.Online);
            this.conversations.data.unshift(this.activeConversation);
          }
        );
      }
  }

  onCreateConversation(senderId: number) {
    if (this.id != senderId) {
      let id = this.id;
      let conversation = this.conversations.data
        .filter(function (obj) {
          return obj.OtherUserId == senderId;
        });
      if (conversation.length == 0) {
        this.usersService.getUser(senderId).subscribe(
          user => {
            let conversation =
              new Conversation('', user.FirstName, user.LastName, 
                '', user.Id, 0, user.AvatarUrl, user.Online);
            this.conversations.data.unshift(conversation);
          }
        );
      }
    }
  }
}

function formatDateTime(dateTime) {
    let d = new Date(dateTime),
        hour = '' + d.getHours(),
        minute = '' + d.getMinutes(),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();
    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;
    if (hour.length < 2) hour = '0' + hour;
    if (minute.length < 2) minute = '0' + minute;
    return hour + ':' + minute + ' ' +
      year + '-' + month + '-' + day;
}
