import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpInterceptor, HttpEvent } from '@angular/common/http';
import { throwError, Observable } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class ErrorInterceptorService  implements HttpInterceptor {
  constructor(public router: Router, private toastrService: ToastrService) {
  }
 
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      retry(1),
      catchError((err) => {
        let message = ""
        if (err.status === 401) {
          message = "Token has expired or you should be logged in"
        }
        else if (err.status === 404) {
          message = "404"
        }
        else if (err.status === 400) {
          message = "400"
        }
        else {
          message = "Unexpected error"
        }
        
        this.toastrService.error(message)
        return throwError(err)
      })
    )
  }
}