import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { EmployeeService } from 'src/app/core/employee.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  companyDepartments$: Observable<object>;

  constructor(private readonly employeeService: EmployeeService) { }

  // TODO:
  ngOnInit() {

  }

  public selectCompanyDepartment(): void {

  }
}
