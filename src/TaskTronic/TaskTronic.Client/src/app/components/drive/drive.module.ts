import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from 'src/app/core/auth.service';
import { DriveComponent } from './drive.component';
import { DriveService } from 'src/app/core/drive.service';
import { DriveBreadcrumbs } from './drive-breadcrumbs/drive-breadcrumbs.component';

@NgModule({
  declarations: [
    DriveComponent,
    DriveBreadcrumbs
  ],
  imports: [CommonModule],
  providers: [DriveService, AuthService]
})
export class DriveModule { }