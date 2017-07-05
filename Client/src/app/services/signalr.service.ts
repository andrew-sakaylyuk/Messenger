import { Injectable, EventEmitter, NgZone } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { Conversation } from '../models/conversation.model';
import { Observable } from 'rxjs/Observable';
import { Data } from '../models/data.model';
import { Message } from '../models/message.model';

declare var $: any;
@Injectable()
export class SignalRService {
    // Declare the variables  
    private proxyName: string = 'messageHub';
    private connection: any;
    private proxy: any;
    // create the Event Emitter  
    public getConversationsEvent: EventEmitter<Data<Array<Conversation>>>;
    public getCorrespondenceEvent: EventEmitter<Data<Array<Message>>>;
    public sendMessageEvent: EventEmitter<Data<Message>>;
    public makeMessagesOldEvent: EventEmitter<Array<Number>>;
    public removeMessageEvent: EventEmitter<Array<any>>;
    public typeMessageEvent: EventEmitter<Number>;
    public notTypeMessageEvent: EventEmitter<Number>;
    public userOnlineEvent: EventEmitter<Number>;
    public userOfflineEvent: EventEmitter<Number>;

    //public connectionEstablished: EventEmitter < Boolean >;  
    public connectionExists: Boolean;
    constructor(private _authService: AuthService, private _ngZone: NgZone) {
        // Constructor initialization  
        //this.connectionEstablished = new EventEmitter < Boolean > ();  
        this.getConversationsEvent = new EventEmitter<Data<Array<Conversation>>>();
        this.getCorrespondenceEvent = new EventEmitter<Data<Array<Message>>>()
        this.sendMessageEvent = new EventEmitter<Data<Message>>();
        this.makeMessagesOldEvent = new EventEmitter<Array<Number>>();
        this.removeMessageEvent = new EventEmitter<Array<any>>();
        this.typeMessageEvent = new EventEmitter<Number>();
        this.notTypeMessageEvent = new EventEmitter<Number>();
        this.userOnlineEvent = new EventEmitter<Number>();
        this.userOfflineEvent = new EventEmitter<Number>();

        this.connectionExists = false;
        // create hub connection 
        this.connection = $.hubConnection(environment.apiUrl);
        this.connection.qs = "Bearer=" + this._authService.getToken();
        // create new proxy as name already given in top  
        this.proxy = this.connection.createHubProxy(this.proxyName);
        // register on server events  
        this.registerOnServerEvents();
        this.refreshConnection();
    }

    // check in the browser console for either signalr connected or not  
    public startConnection(): void {

        this.connection.start().done((data: any) => {
            console.log('Now connected ' + data.transport.name + 
                ', connection ID= ' + data.id);
            //this.connectionEstablished.emit(true);  
            this.connectionExists = true;
        }).fail((error: any) => {
            console.log('Could not connect ' + error);
            //this.connectionEstablished.emit(false);  
        });
    }

    public restartConnection(): void {
        this.connection.start();
    }

    public stopConnection(): void {
        this.connectionExists = false;
        this.connection.stop();
        console.log('Stop connection');
    }

    private registerOnServerEvents(): void {
        this.proxy.on('onGetConversations', 
            (chats: Array<Conversation>, pageCount: Number) => {
            this.getConversationsEvent.emit(
                new Data<Array<Conversation>>(chats, pageCount));
        });
        this.proxy.on('onGetCorrespondence', 
            (messages: Array<Message>, pageCount: Number) => {
            this.getCorrespondenceEvent.emit(
                new Data<Array<Message>>(messages, pageCount));
        });
        this.proxy.on('onSendMessage', 
            (message: Message, pageCount: Number) => {
            this.sendMessageEvent.emit(
                new Data<Message>(message, pageCount));
        });
        this.proxy.on('onMakeMessagesOld', (messagesIds: Array<Number>) => {
            this.makeMessagesOldEvent.emit(messagesIds);
        });
        this.proxy.on('onRemoveMessageU', 
            (senderId: Number,messageId: Number,pageCount:Number) => {
            this.removeMessageEvent.emit([senderId,messageId,pageCount]);
        });
        this.proxy.on('onTypeMessage', (senderId:Number) => {
            this.typeMessageEvent.emit(senderId);
        });
        this.proxy.on('onNotTypeMessage', (senderId:Number) => {
            this.notTypeMessageEvent.emit(senderId);
        });
        this.proxy.on('onUserOnline', (senderId:Number) => {
            this.userOnlineEvent.emit(senderId);
        });
        this.proxy.on('onUserOffline', (senderId:Number) => {
            this.userOfflineEvent.emit(senderId);
        });
    }

    public getNewMessagesCount(): Promise<number> {
        return this.proxy.invoke('GetNewMessagesCount');
    }

    public getNewMessagesCountWith(userId:Number): Promise<number> {
        return this.proxy.invoke('GetNewMessagesCountWith',userId);
    }

    public getConversations(page=1):void {
      this.proxy.invoke('GetConversations',page);
    }

    public getCorrespondence(userId:Number, page=1):void {
      this.proxy.invoke('GetCorrespondence',userId, page);
    }

    public sendMessage(receiverId: Number, text: string): void {
        this.proxy.invoke('SendMessage', receiverId, text);
    }

    public makeMessagesOld(readMessages: Array<Number>): void {
        this.proxy.invoke('MakeMessagesOld', readMessages);
    }

    public removeMessage(id: Number): void {
        this.proxy.invoke('RemoveMessageU', id);
    }

    public typeMessage(receiverId: Number) {
        this.proxy.invoke('TypeMessage', receiverId);
    }

    public notTypeMessage(receiverId: Number) {
        this.proxy.invoke('NotTypeMessage', receiverId);
    }

    public userOnline() {
        this.proxy.invoke('UserOnline');
    }

    public userOffline() {
        this.proxy.invoke('UserOffline');
    }

    public refreshConnection() {
        //if client is not active too long, signalr connection will be lost
        //so we need to start again
        var tryingToReconnect = false;
        this.connection.reconnecting(function () {
            tryingToReconnect = true;
        });
        this.connection.reconnected(function () {
            tryingToReconnect = false;
        });
        this.connection.disconnected(function () {
            if (tryingToReconnect) {
                setTimeout(function () {
                    this.connection.start();
                }, 5000);// Restart connection after 5 seconds.
            }
        });
    }
}