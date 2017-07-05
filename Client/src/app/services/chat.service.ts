import { Injectable, OnInit, Inject, EventEmitter, NgZone } from '@angular/core';
import { Http, Headers } from '@angular/http';
import { SignalRService } from "../services/signalr.service";
import { Data } from '../models/data.model';
import { Observable } from 'rxjs/Observable';
import { Conversation } from '../models/conversation.model';
import { Message } from '../models/message.model';
import { environment } from '../../environments/environment';

@Injectable()
export class ChatService {

  private readonly apiUrl = environment.apiUrl;

  public api = 'api/chat';
  private header = new Headers({ 'Content-Type': 'application/json' });

  public newMessagesCountEvent: EventEmitter<Number>;
  public getConversationsEvent: EventEmitter<Data<Array<Conversation>>>;
  public getCorrespondenceEvent: EventEmitter<Data<Array<Message>>>;
  public sendMessageEvent: EventEmitter<Data<Message>>;
  public makeMessagesOldEvent: EventEmitter<Array<Number>>;
  public removeMessageEvent: EventEmitter<Array<any>>;
  public typeMessageEvent: EventEmitter<any>;
  public notTypeMessageEvent: EventEmitter<any>;
  public userOnlineEvent: EventEmitter<Number>;
  public userOfflineEvent: EventEmitter<Number>;
  public newMessagesCount:number;
  private audio: any;
  private sendMessageSubscription: any;
  private id: Number;

  constructor(private http: Http, private _signalRService: SignalRService, 
    private _ngZone: NgZone) {
    this.newMessagesCountEvent = new EventEmitter<Number>();
    this.getConversationsEvent = _signalRService.getConversationsEvent;
    this.getCorrespondenceEvent = _signalRService.getCorrespondenceEvent;
    this.sendMessageEvent = _signalRService.sendMessageEvent;
    this.makeMessagesOldEvent = _signalRService.makeMessagesOldEvent;
    this.removeMessageEvent = _signalRService.removeMessageEvent;
    this.typeMessageEvent = _signalRService.typeMessageEvent;
    this.notTypeMessageEvent = _signalRService.notTypeMessageEvent;
    this.userOnlineEvent = _signalRService.userOnlineEvent;
    this.userOfflineEvent = _signalRService.userOfflineEvent;
    this.audio = new Audio('assets/new-message-tone.mp3');
    this.id = JSON.parse(sessionStorage.getItem('user'))["Id"];

    this.sendMessageSubscription =
      this.sendMessageEvent.subscribe((data: Data<Message>) => {
        this.getNewMessagesCount();
        let otherUserId = +this.getQueryString('id', window.location.href);
        let message = data.data;
        if (otherUserId !== message.SenderId && this.id != message.SenderId) {
          this.newMessagePlay();
        }
      });

    this.newMessagesCountEvent.subscribe((newMessagesCount: number) => {
      this._ngZone.run(() => {
        this.newMessagesCount = newMessagesCount;
      });
    });

      this.removeMessageEvent.subscribe((messageInfo: Array<any>) => {
        this._ngZone.run(() => {
            this.getNewMessagesCount();
        });
      });
  }

  getNewMessagesCount(): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.getNewMessagesCount();
      }, 1000);
    }
    else {
      this._signalRService.getNewMessagesCount().then((data: number) => {
        this.newMessagesCountEvent.emit(data);

      });
    }
  }

  getNewMessagesCountWith(userId: number): Promise<number> {
    let promise: Promise<number>;
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        promise = this.getNewMessagesCountWith(userId);
      }, 1000);
    }
    else {
      promise = this._signalRService.getNewMessagesCountWith(userId);
    }
    return promise;
  }

  getConversations(page = 1): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.getConversations(page);
      }, 1000);
    }
    else {
      this._signalRService.getConversations(page);
    }
  }

  getCorrespondence(userId: Number, page = 1): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.getCorrespondence(userId, page);
      }, 1000);
    }
    else {
      this._signalRService.getCorrespondence(userId, page);
    }
  }

  sendMessage(receiverId: Number, text: string): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.sendMessage(receiverId, text);
      }, 1000);
    }
    else {
      this._signalRService.sendMessage(receiverId, text);
    }
  }

  public makeMessagesOld(readMessages: Array<Number>): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.makeMessagesOld(readMessages);
      }, 1000);
    }
    else {
      this._signalRService.makeMessagesOld(readMessages);
    }
  }

  public removeMessage(id: Number): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.removeMessage(id);
      }, 1000);
    }
    else {
      this._signalRService.removeMessage(id);
    }
  }

  public typeMessage(receiverId: Number): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.typeMessage(receiverId);
      }, 1000);
    }
    else {
      this._signalRService.typeMessage(receiverId);
    }
  }

  public notTypeMessage(receiverId: Number): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(this.notTypeMessage(receiverId), 1000);
    }
    else {
      this._signalRService.notTypeMessage(receiverId);
    }
  }

  public userOnline(): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.userOnline();
      }, 1000);
    }
    else {
      this._signalRService.userOnline();
    }
  }

  public userOffline(): void {
    if (this._signalRService.connectionExists == false) {
      setTimeout(() => {
        this.userOffline();
      }, 1000);
    }
    else {
      this._signalRService.userOffline();
    }
  }

  public connectionExists(): Boolean {
    return this._signalRService.connectionExists;
  }

  public startConnection(): void {
    this._signalRService.startConnection();
  }

  public restartConnection(): void {
    this._signalRService.restartConnection();
  }

  public stopConnection(): void {
    this._signalRService.stopConnection();
  }

  public newMessagePlay() {
    this.audio.play();
  }

  /**
 * Get the value of a querystring
 * @param  {String} field The field to get the value of
 * @param  {String} url   The URL to get the value from (optional)
 * @return {String}       The field value
 */
  public getQueryString(field, url) {
    var href = url ? url : window.location.href;
    var reg = new RegExp('[?&]' + field + '=([^&#]*)', 'i');
    var string = reg.exec(href);
    return string ? string[1] : null;
  }

}
