import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/members';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { AccountService } from '../../_services/account.service';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {

  private accounService = inject(AccountService)
  private memberService = inject(MembersService);
  member = input.required<Member>();
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChange = output<Member>();
  ngOnInit(): void {
    this.initializeUploader();
  }
  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  deletePhoto(photo: Photo) {

    this.memberService.deletePhoto(photo).subscribe({
      next: _ => {
        const updatedMember = { ...this.member() };
        updatedMember.photos = updatedMember.photos.filter(x => x.id != photo.id);
        this.memberChange.emit(updatedMember);
      }

    })
  }



  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: _ => {
        const user = this.accounService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accounService.setCurrentUser(user);

        }

        const updatedMember = { ...this.member() }
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        })
        this.memberChange.emit(updatedMember);

      }
    })
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accounService.currentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,

    });
    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false
    }
    this.uploader.onSuccessItem = (item, response, status, Headers) => {
      const photo = JSON.parse(response);
      const updateMember = { ...this.member() }
      updateMember.photos.push(photo);
      this.memberChange.emit(updateMember);

    }
  }

}