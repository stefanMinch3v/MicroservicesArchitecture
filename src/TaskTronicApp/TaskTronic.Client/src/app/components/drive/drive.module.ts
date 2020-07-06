import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from 'src/app/core/auth.service';
import { DriveComponent } from './drive.component';
import { DriveService } from 'src/app/core/drive.service';
import { DriveBreadcrumbs } from './drive-breadcrumbs/drive-breadcrumbs.component';
import { FileSizeDisplayPipe } from 'src/app/core/file-size-display.pipe';
import { FaIconPipe } from 'src/app/core/fa-icons.pipe';

@NgModule({
  declarations: [
    DriveComponent,
    DriveBreadcrumbs,
    FileSizeDisplayPipe,
    FaIconPipe
  ],
  imports: [CommonModule],
  providers: [DriveService, AuthService]
})
export class DriveModule { }