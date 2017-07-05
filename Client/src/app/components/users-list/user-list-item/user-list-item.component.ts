import {
  Component,
  OnInit,
  Input
} from '@angular/core';

@Component({
  selector: 'am-user-list-item',
  templateUrl: './user-list-item.component.html',
  styleUrls: ['./user-list-item.component.scss']
})
export class UserListItemComponent implements OnInit {

  @Input() username: string;
  @Input() image: string;
  @Input() id: number;
  @Input() online: boolean;

  constructor() {
    this.username = "";
    this.image = "";
    this.id = 0;
    this.online = false;
  }

  ngOnInit() {}

}
