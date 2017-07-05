import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../../models/user.model';

@Component({
  selector: 'am-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {

  user: User;
  reloading: boolean;

  constructor(private router: Router, private cd: ChangeDetectorRef) { 
	  this.user = new User();
  	this.reloading = false;
  }

  ngOnInit() {
  }

  onSearch(){
  	this.reloading = true;
  	this.cd.detectChanges();
  	this.reloading = false;
  	this.cd.detectChanges();
    this.cd.markForCheck();
  }

}
