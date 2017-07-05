import {
  Component,
  Input,
  Output,
  Inject,
  OnInit,
  NgZone,
  ElementRef,
  ViewChild,
  OnDestroy,
  EventEmitter
} from '@angular/core';

import { Conversation } from '../../../models/conversation.model';
import { Message } from '../../../models/message.model';
import { Data } from '../../../models/data.model';

declare var $: any;
@Component({
  selector: 'am-correspondence',
  templateUrl: './correspondence.component.html',
  styleUrls: ['./correspondence.component.scss']
})
export class CorrespondenceComponent implements OnInit, OnDestroy { //
  @ViewChild('chatScroll') private myScrollContainer: ElementRef;
  @ViewChild('textarea') private textArea: ElementRef;

  @Input() chatSidenav;
  @Input() activeConversation: Conversation;
  @Input() noConversations: Boolean;
  @Input() conversations: Data<Array<Conversation>>;

  @Output() onCreateConversation: EventEmitter<any>;
  readonly TYPEMESSAGESTR: string = " is typing...";
  readonly MINMESSAGESONVIEW: Number = 15;

  messages: Data<Array<Message>>;
  newMessageText: string;
  avatarUrl: string;
  firstName: string;
  id: Number;
  correspondenceSubscription: any;
  sendMessageSubscription: any;
  makeMessagesOldSubscription: any;
  removeMessageSubscription: any;
  typeMessageSubscription: any;
  notTypeMessageSubscription: any;
  userOnlineSubscription: any;
  userOfflineSubscription: any;
  readMessages: Array<Number>;
  typingMessage: Boolean;
  typeMessageStr: string = '';
  currentPage: Number;
  pageCount: Number;
  currentUserId: Number;
  scrollPos: Number;
  pagesLeft: Number;
  getMoreMessagesInProcess: Boolean;

  constructor( @Inject('ChatService') private chatService, private _ngZone: NgZone) {
    this.onCreateConversation = new EventEmitter<any>();
    this.avatarUrl = JSON.parse(sessionStorage.getItem('user'))["AvatarUrl"];
    this.firstName = JSON.parse(sessionStorage.getItem('user'))["FirstName"];
    this.id = JSON.parse(sessionStorage.getItem('user'))["Id"];
    this.messages = new Data<Array<Message>>([], 0);
    this.readMessages = new Array<Number>();
    this.typingMessage = false;
    this.currentPage = 1;
    this.pageCount = 1;
    this.currentUserId = 0;
    this.newMessageText = '';
    this.scrollPos = 0;
    this.pagesLeft = 0;
    this.getMoreMessagesInProcess = false;
  }

  ngOnInit() {
    this.userOnlineSubscription =
    this.chatService.userOnlineEvent.subscribe((onlineUserId:number)=>{
        this._ngZone.run(() => {
          let conversation : Conversation = this.findConversationOnUI(onlineUserId);
          if (conversation!=null)
            conversation.Online=true;
        });
    });
    this.userOfflineSubscription =
    this.chatService.userOfflineEvent.subscribe((offlineUserId:number)=>{
        this._ngZone.run(() => {
          let conversation : Conversation = this.findConversationOnUI(offlineUserId);
          if (conversation!=null)
            conversation.Online=false;
        });
    });

    this.correspondenceSubscription =
      this.chatService.getCorrespondenceEvent.subscribe((data: Data<Array<Message>>) => {
        //for scroll to stay still
        if (this.myScrollContainer.nativeElement.scrollTop == 0) {
          this.myScrollContainer.nativeElement.scrollTop = 1
        }

        this._ngZone.run(() => {
          this.typeMessageStr = '';
          let activeUserId = +this.activeConversation.OtherUserId
          if (this.currentUserId != activeUserId) {
            this.messages = new Data<Array<Message>>([], 0);
            this.currentUserId = activeUserId;
            this.currentPage = 1;
          }
          if (this.pagesLeft > 1) {
            let remainder = this.messages.data.length % data.data.length;
            if (remainder != 0)
              this.messages.data.splice(0, remainder);
          }
          this.pageCount = +data.pageCount;

          this.pagesLeft = +this.pageCount - +this.currentPage;

          // add new to the beginning
          for (let i = data.data.length - 1; i >= 0; i--) {
            data.data[i].DateTime = formatDateTime(
              new Date(data.data[i].DateTime +' UTC'));
            this.messages.data.unshift(data.data[i]);
          }
        });
        data.data.forEach(message => {
          if (this.id !== message.SenderId && message.New == true) {
            this.readMessages.push(message.Id);
          }
        });
        if (this.currentPage < 2) {
          setTimeout(this.scrollToPos(this.myScrollContainer.nativeElement.scrollHeight), 0);
          setTimeout(() => {
            if (this.scrollPos == 0)
              this.scrollPos = this.myScrollContainer.nativeElement.scrollHeight;
          }, 0);
        }
        this.makeMessagesOld();
        this.updateNewMessagesCountOnGetCorrespondence();
        this.chatService.getNewMessagesCount();
        this.getMoreMessagesInProcess = false;

      });

    this.sendMessageSubscription =
      this.chatService.sendMessageEvent.subscribe((messageInfo: Data<Message>) => {
        let message = messageInfo.data;
        message.DateTime = formatDateTime(new Date(message.DateTime +' UTC'));
        this._ngZone.run(() => {
          this.pageCount = messageInfo.pageCount;
          let conversation = this.conversations.data
            .filter(function (obj) {
              return obj.OtherUserId == message.SenderId;
            });
          if (conversation.length > 0) {
            conversation[0].DateTime = message.DateTime;
            conversation[0].Text = message.Text;
          }
          //if i'm not the sender and chat is active with other user, who calls this method
          if (this.id !== message.SenderId
            && this.activeConversation
            && message.SenderId == this.activeConversation.OtherUserId) {
            //update view
            this.activeConversation.DateTime = message.DateTime;
            this.activeConversation.Text = message.Text;
            
            this.messages.data.push(message);
            this.readMessages.push(message.Id);
            this.makeMessagesOld();
            this.currentPage = +this.pageCount - +this.pagesLeft;
            this.chatService.getNewMessagesCount();
          }
          //if i'm the sender
          else if (this.id == message.SenderId) {
            this.messages.data.push(message);
            this.currentPage = +this.pageCount - +this.pagesLeft;
            //update view
            this.activeConversation.DateTime = message.DateTime;
            this.activeConversation.Text = message.Text;
            this.putConversationOnTop();
          }
          //if i'm not the sender
          if (this.id !== message.SenderId) {
            //create on other user client new conversation
            this.onCreateConversation.emit(message.SenderId);
          }
          //if i'm not the sender and chat is not active with other user, who calls this method
          if (this.id !== message.SenderId
            && this.activeConversation
            && message.SenderId != this.activeConversation.OtherUserId) {
            this.updateNewMessagesCountIfReceiver(message.SenderId);
          }
        });
            setTimeout(this.scrollToPos(this.myScrollContainer.nativeElement.scrollHeight));
      });

    this.makeMessagesOldSubscription =
      this.chatService.makeMessagesOldEvent.subscribe((readMessages: Array<Number>) => {
        this._ngZone.run(() => {
          this.messages.data.forEach(message => {
            readMessages.forEach(messageId => {
              if (messageId == message.Id) message.New = false;
            });
          });
        });
      });

    this.removeMessageSubscription =
      this.chatService.removeMessageEvent.subscribe((messageInfo: Array<any>) => {
        console.log('remove')
        this._ngZone.run(() => {
          let senderId = messageInfo[0];
          let messageId = messageInfo[1];
          this.pageCount = messageInfo[2];
          if (this.currentPage > this.pageCount) {
            this.currentPage = this.pageCount;
          }

          //if i'm not the sender and chat is active with other user, who calls this method
          if (this.id !== senderId
            && this.activeConversation
            && senderId == this.activeConversation.OtherUserId) {
            this.pagesLeft = +this.pageCount - +this.currentPage;
            this.removeMessageFromUI(messageId);
            this.updateConversation();
          }
          //if i'm the sender
          else if (this.id == senderId) {
            this.pagesLeft = +this.pageCount - +this.currentPage;
            this.removeMessageFromUI(messageId);
            this.updateConversation();
          }
          //if i'm not the sender and chat is not active with other user, who calls this method
          if (this.id !== senderId
            && this.activeConversation
            && senderId != this.activeConversation.OtherUserId) {
            this.updateNewMessagesCountIfReceiver(senderId);
            this.updateConversation();
          }
        });

        if (this.messages.data.length == 0)
          this.chatService.getCorrespondence(this.activeConversation.OtherUserId, this.currentPage);
      });

    this.typeMessageSubscription =
      this.chatService.typeMessageEvent.subscribe((senderId: Number) => {
        this._ngZone.run(() => {
          if (this.activeConversation && senderId == this.activeConversation.OtherUserId)
            this.typeMessageStr = this.activeConversation.FirstName + " "
              + this.activeConversation.LastName + this.TYPEMESSAGESTR;
        });
      });

    this.notTypeMessageSubscription =
      this.chatService.notTypeMessageEvent.subscribe((senderId: Number) => {
        this._ngZone.run(() => {
          if (this.activeConversation && senderId == this.activeConversation.OtherUserId)
            this.typeMessageStr = '';
        });
      });
  }

  updateConversation() {
    if (this.messages.data.length > 0) {
      this.activeConversation.DateTime = formatDateTime(
        new Date( this.messages.data[this.messages.data.length - 1]
          .DateTime +' UTC'));
      this.activeConversation.Text =
        this.messages.data[this.messages.data.length - 1].Text;
    }
    else {
      this.activeConversation.DateTime = '';
      this.activeConversation.Text = '';
    }
  }

  putConversationOnTop() {
    let index = -1;
    for (let i = 0; i < this.conversations.data.length; i++) {
      if (this.conversations.data[i].OtherUserId == this.activeConversation.OtherUserId) {
        index = i;
        break;
      }
    }
    if (index != -1 && index != 0) {
      let conversation: Conversation = this.conversations.data[index];
      this.conversations.data.splice(index, 1);
      this.conversations.data.unshift(conversation);
    }
  }

  updateNewMessagesCountOnGetCorrespondence() {
    this.chatService.getNewMessagesCountWith(this.activeConversation.OtherUserId)
      .then((data: any) => {
        let newMessagesCount = data.Second;
        this.activeConversation.NewMessagesCount = newMessagesCount;
      });
  }

  updateNewMessagesCountIfReceiver(senderId: Number) {
    this.chatService.getNewMessagesCountWith(senderId)
      .then((data: any) => {
        this._ngZone.run(() => {
          let senderId = data.First;

          let newMessagesCount = data.Second;
          let conversation = this.findConversationOnUI(senderId);
          if (conversation != null) {
            conversation.NewMessagesCount = newMessagesCount;
          }
        });
      });
  }

  findConversationOnUI(userId: number): Conversation {
    let conversation = this.conversations.data
      .filter(function (c) {
        return c.OtherUserId == userId;
      });
    if (conversation.length > 0) {
      return conversation[0];
    }
    else return null;
  }

  removeMessageFromUI(messageId: Number) {
    let index = -1;
    for (let i = 0; i < this.messages.data.length; i++) {
      if (this.messages.data[i].Id == messageId) {
        index = i;
        break;
      }
    }
    if (index != -1)
      this.messages.data.splice(index, 1);
  }

  ngOnDestroy() {
    this.correspondenceSubscription.unsubscribe();
    this.sendMessageSubscription.unsubscribe();
    this.makeMessagesOldSubscription.unsubscribe();
    this.removeMessageSubscription.unsubscribe();
    this.typeMessageSubscription.unsubscribe();
    this.notTypeMessageSubscription.unsubscribe();
    this.userOnlineSubscription.unsubscribe();
    this.userOfflineSubscription.unsubscribe();
    //to be sure user is not typing
    if (this.chatService.connectionExists() == true && this.activeConversation) {
      this.notTypeMessage();
    }
  }

  scrollToPos(pos: Number): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = pos;
    } catch (err) { console.log('error with scroll') }
  }

  sendMessage() {
    this.chatService.notTypeMessage(this.activeConversation.OtherUserId);
    this.newMessageText = this.newMessageText.replace(/\n/g, "");
    if (this.activeConversation && this.newMessageText) {
      this.chatService.sendMessage(this.activeConversation.OtherUserId, this.newMessageText);
    }
    this.newMessageText = '';
    this.textArea.nativeElement.focus();
  }

  pressKeyTextArea(): void {
    if (this.newMessageText == "") {
      if (this.typingMessage == true) {
        this.notTypeMessage();
        this.typingMessage = false;
      }
    }
    if (this.newMessageText != "") {
      //every few symbols call this method
      if (this.typingMessage == false
        || (this.newMessageText.length % 5 == 0 && this.newMessageText.length != 0)) {
        this.typeMessage();
        this.typingMessage = true;
      }
    }
  }

  typeMessage(): void {
    this.chatService.typeMessage(this.activeConversation.OtherUserId);
  }

  notTypeMessage(): void {
    this.chatService.notTypeMessage(this.activeConversation.OtherUserId);
  }

  makeMessagesOld(): void {
    this.chatService.makeMessagesOld(this.readMessages);
    this.readMessages = new Array<Number>();
  }

  removeMessage(id: Number) {
    this.chatService.removeMessage(id);
  }

  onChatSideTriggered() {
    this.chatSidenav.toggle();
  }

  showDelButton(id: Number) {
    let display = $('#' + id).css('display');
    if (display == 'none') $('#' + id).fadeIn("slow");
    else $('#' + id).fadeOut("slow");
  }

  getMoreMessages() {
    if (!this.getMoreMessagesInProcess
      && this.myScrollContainer.nativeElement.scrollTop < 100
      && this.pagesLeft > 0) {
      this.currentPage = +this.currentPage + 1;
      this.chatService.getCorrespondence(this.activeConversation.OtherUserId, 
        this.currentPage);
      this.getMoreMessagesInProcess = true;
    }
  }

}

function formatDateTime(dateTime) {
    let d = new Date(dateTime),
        year = d.getFullYear(),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        hour = '' + d.getHours(),
        minute = '' + d.getMinutes(),
        second = '' + d.getSeconds()
        ;
    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;
    if (hour.length < 2) hour = '0' + hour;
    if (minute.length < 2) minute = '0' + minute;
    if (second.length < 2) second = '0' + second;
    return hour + ':' + minute + ':' + second + ' ' +
      year + '-' + month + '-' + day;
}