import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { map } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class EmployeeService {
    private readonly DRIVE_EMPLOYEES = '/Employees/';
    constructor(private http: HttpClient) { }

    public register(email: string): Observable<any> {
        const url = environment.driveUrl + `${this.DRIVE_EMPLOYEES}Create`;
        return this.http.post(url, email);
    }

    public getCompanyDepartmentsSignId(): Observable<number> {
        const url = environment.driveUrl + '/Employees/GetCompanyDepartmentSignId';
        return this.http.get(url)
            .pipe(map((response: number) => response));
    }
}
