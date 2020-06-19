import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DriveComponent } from './drive.component';
import { AuthGuard } from 'src/app/core/auth.guard';

const routes: Routes = [
  { 
    path: ':companyId/:departmentId', 
    component: DriveComponent, 
    canActivate: [AuthGuard],
    data: { kind: 'root' } 
  },
  { 
    path: ':companyId/:departmentId/:selectedFolderId', 
    component: DriveComponent, 
    canActivate: [AuthGuard],
    data: { kind: 'folder' } 
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DriveRoutingModule { }
