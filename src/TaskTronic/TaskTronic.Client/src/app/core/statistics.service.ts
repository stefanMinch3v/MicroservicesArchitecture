import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class StatisticsService {
    private readonly FOLDER_VIEWS = '/FolderViews/';

    constructor(private http: HttpClient) { }

    addFolderView(folderId: number): Observable<any> {
        const url = environment.statisticsUrl + `${this.FOLDER_VIEWS}AddView`;

        return this.http.post(url, {}, {
            params: {
                folderId: folderId.toString()
            }});
    }
}
