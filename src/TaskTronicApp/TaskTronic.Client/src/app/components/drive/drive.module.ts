import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from 'src/app/core/auth.service';
import { DriveComponent } from './drive.component';
import { DriveService } from 'src/app/core/drive.service';
import { DriveBreadcrumbs } from './drive-breadcrumbs/drive-breadcrumbs.component';
import { FileSizeDisplayPipe } from 'src/app/core/file-size-display.pipe';
import { FaIconPipe } from 'src/app/core/fa-icons.pipe';
import { AppendFolderPath } from 'src/app/core/append-folder-path.pipe';
import { FaIconColorPipe } from 'src/app/core/fa-icons-color.pipe';
import { ModalComponent } from '../modal/modal.component';

@NgModule({
  declarations: [
    DriveComponent,
    DriveBreadcrumbs,
    FileSizeDisplayPipe,
    FaIconPipe,
    AppendFolderPath,
    FaIconColorPipe,
    ModalComponent
  ],
  imports: [CommonModule],
  providers: [DriveService, AuthService]
})
export class DriveModule { }
