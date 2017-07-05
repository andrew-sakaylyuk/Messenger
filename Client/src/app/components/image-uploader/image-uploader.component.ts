import { Component, Input } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import { UploadService } from '../../services/upload.service';
import { MdSnackBar } from '@angular/material';

@Component({
  selector: 'am-image-uploader',
  templateUrl: 'image-uploader.component.html',
  styleUrls: ['image-uploader.component.scss'],
})
export class ImageUploaderComponent {

  @Input() width: number;
  @Input() height: number;
  @Input() type: string;
  @Input() image: string;
  file: File;

  constructor(private uploadService: UploadService, public snackBar: MdSnackBar) {
    this.width = 120;
    this.height = 120;
    this.type = "square";
    this.image = JSON.parse(sessionStorage.getItem('user'))['AvatarUrl'];
  }

  fileChange(event) {
    let fileList = event.target.files;
    if(fileList.length > 0) {
        this.file = fileList[0];
        let reader = new FileReader();
        reader.onload = (e: any) => {
            this.image = e.target.result;
        };
        reader.readAsDataURL(fileList[0]);
        this.uploadService.uploadFile(this.file)
        .subscribe( data => {
          this.snackBar.open("Avatar uploaded successfully!", 
          "Close", {duration: 3000, });
          /* setTimeout( () => { 
            this.image = JSON.parse(sessionStorage.getItem('user'))['AvatarUrl'];
          }, 2000); */
        }, error => console.log(error) );   
    }
  }

  public getFile(): File {
    return this.file;
  }

  public getImage(): string {
    return this.image;
  }

}