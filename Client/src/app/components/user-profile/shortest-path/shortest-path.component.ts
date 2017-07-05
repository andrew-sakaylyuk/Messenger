import { 
  Component, 
  Input, 
  OnInit, 
  SimpleChanges, 
  OnChanges, 
  ChangeDetectorRef 
} from '@angular/core';

@Component({
  selector: 'am-shortest-path',
  templateUrl: './shortest-path.component.html',
  styleUrls: ['./shortest-path.component.scss']
})
export class ShortestPathComponent implements OnInit, OnChanges {

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
