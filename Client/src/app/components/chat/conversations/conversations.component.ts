import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, Inject, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { Data } from '../../../models/data.model';
import { Conversation } from '../../../models/conversation.model';

@Component({
  selector: 'am-conversations',
  templateUrl: './conversations.component.html',
  styleUrls: ['./conversations.component.scss']
})
export class ConversationsComponent implements OnInit {

  @Input() conversations: Data<Array<Conversation>>;
  @Output() onActiveConversation = new EventEmitter();

  private userId: Number;

  constructor( @Inject('ChatService') private chatService,
    private router: Router, private _ngZone: NgZone) {
  }

  ngOnInit() {
  }

  setActiveConversation(conversation: Conversation) {
    this.userId = +this.chatService.getQueryString('id', window.location.href);
    if (this.userId != conversation.OtherUserId) {
      this.router.navigate(['/chats'], {
        queryParams:
        { id: conversation.OtherUserId }
      });
      this.onActiveConversation.emit(conversation);
      this.chatService.getCorrespondence(conversation.OtherUserId);
    }
  }

}
