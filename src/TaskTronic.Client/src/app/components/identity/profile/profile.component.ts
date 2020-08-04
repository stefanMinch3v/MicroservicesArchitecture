import { Component, OnInit } from '@angular/core';
import { EmployeeService } from 'src/app/core/employee.service';
import { NotificationService } from 'src/app/core/notification.service';
import { CompanyWrapper } from 'src/app/core/models/company-wrapper.model';
import { SelectedCompanyModel } from 'src/app/core/models/selected-company.model';
import { CompanyService } from 'src/app/core/company.service';
import { TotalView } from 'src/app/core/models/total-view.model';
import { GatewayService } from 'src/app/core/gateway.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  companyWrapper: CompanyWrapper;
  totalViews: Array<TotalView>;

  constructor(
    private readonly employeeService: EmployeeService,
    private readonly notificationService: NotificationService,
    private readonly companyService: CompanyService,
    private readonly gatewayService: GatewayService) { }

  ngOnInit() {
    this.companyService.getCompanies()
      .subscribe(companyWrapper => this.companyWrapper = companyWrapper);

    this.gatewayService.getTotalViews()
      .subscribe(views => this.totalViews = views);
  }

  public selectCompanyDepartment(companyId: number, departmentId: number): void {
    if (companyId < 1 || departmentId < 1) {
      this.notificationService.errorMessage('Invalid company/department.');
    }

    this.employeeService.setCompany(companyId, departmentId)
      .subscribe(_ => {
        this.notificationService.successMessage('Data saved.');

        this.companyService.getCompanies()
          .subscribe(companyWrapper => this.companyWrapper = companyWrapper);
      });
  }

  public selectedDataMatch(companyId: number, departmentId: number, selectedData: SelectedCompanyModel): boolean {
    if (!selectedData) {
      return false;
    }

    if (selectedData.companyId === companyId && selectedData.departmentId === departmentId) {
      return true;
    }

    return false;
  }
}