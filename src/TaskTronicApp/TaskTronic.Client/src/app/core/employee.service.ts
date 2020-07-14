import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class EmployeeService {
    constructor(
        private http: HttpClient) { }

    public register(email: string): Observable<any> {
        const url = environment.driveUrl + '/employees/create';
        return this.http.post(url, email);
    }
}
