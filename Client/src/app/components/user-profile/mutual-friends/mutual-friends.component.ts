import { 
  Component, 
  Input, 
  OnInit, 
  SimpleChanges, 
  OnChanges, 
  ChangeDetectorRef 
} from '@angular/core';

@Component({
  selector: 'am-mutual-friends',
  templateUrl: './mutual-friends.component.html',
  styleUrls: ['./mutual-friends.component.scss']
})
export class MutualFriendsComponent implements OnInit, OnChanges {
  
  @Input() userId: number;
  reloading: boolean;

  constructor(private cd: ChangeDetectorRef) {
  	this.reloading = false;
  }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges) {
    this.userId = changes.userId.currentValue;
    this.reloading = true;
  	this.cd.detectChanges();
  	this.reloading = false;
  	this.cd.detectChanges();
    this.cd.markForCheck();
  }

}
