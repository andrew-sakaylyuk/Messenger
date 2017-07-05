import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { CommonModule } from '@angular/common';
import { FlexLayoutModule } from "@angular/flex-layout";
import { PerfectScrollbarModule, PerfectScrollbarConfigInterface } from "ngx-perfect-scrollbar";

import 'hammerjs';
import {NgPipesModule} from 'ngx-pipes';

import { BrowserAnimationsModule, NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from '@angular/material';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavigationComponent } from './components/navigation/navigation.component';
import { SearchComponent } from './components/search/search.component';
import { ProfileComponent, ConfirmDeleteAccountDialog} from './components/profile/profile.component';
import { AuthComponent } from './components/auth/auth.component';
import { SigninComponent } from './components/auth/signin/signin.component';
import { SignupComponent } from './components/auth/signup/signup.component';
import { AuthService } from './services/auth.service';
import { ChangeAccountService } from './services/change-account.service';
import { UploadService } from './services/upload.service';
import { SignalRService } from './services/signalr.service';
import { HttpClient } from './services/http-client';
import { ImageUploaderComponent } from './components/image-uploader/image-uploader.component';
import { FriendsComponent } from './components/friends/friends.component';
import { UserProfileComponent } from './components/user-profile/user-profile.component';
import { MutualFriendsComponent } from './components/user-profile/mutual-friends/mutual-friends.component';
import { ShortestPathComponent } from './components/user-profile/shortest-path/shortest-path.component';
import { UsersListComponent } from './components/users-list/users-list.component';
import { UserListItemComponent } from './components/users-list/user-list-item/user-list-item.component';
import { UserActiveButtonsComponent } from './components/user-active-buttons/user-active-buttons.component';
import { ChatService } from './services/chat.service';
import { ChatComponent } from './components/chat/chat.component';
import { ConversationsComponent } from './components/chat/conversations/conversations.component';
import { CorrespondenceComponent } from './components/chat/correspondence/correspondence.component';
import { AuthGuard } from './auth.guard';

const perfectScrollbarConfig: PerfectScrollbarConfigInterface = {
  suppressScrollX: true
};

@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,
    SearchComponent,
    ProfileComponent,
    AuthComponent,
    SigninComponent,
    SignupComponent,
    ImageUploaderComponent,
    ConfirmDeleteAccountDialog,
    FriendsComponent,
    UserProfileComponent,
    MutualFriendsComponent,
    ShortestPathComponent,
    UsersListComponent,
    UserListItemComponent,
    UserActiveButtonsComponent,
    ChatComponent,
    ConversationsComponent,
    CorrespondenceComponent
  ],
  entryComponents: [
    ConfirmDeleteAccountDialog
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    BrowserAnimationsModule,
    NoopAnimationsModule,
    MaterialModule,
    AppRoutingModule,
    FlexLayoutModule,
    CommonModule,
    PerfectScrollbarModule.forRoot(perfectScrollbarConfig),
    NgPipesModule
  ],
  providers: [
    AuthService, 
    ChangeAccountService, 
    UploadService, 
    HttpClient, 
    SignalRService,
    { provide: 'ChatService', useClass: ChatService },
    AuthGuard
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
