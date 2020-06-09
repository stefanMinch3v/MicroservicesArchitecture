import { Component, OnInit } from '@angular/core';
import { RegisterModelForm } from './register.model';
import { FormGroup, FormBuilder } from 'ngx-strongly-typed-forms';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { IdentityService } from 'src/app/core/identity.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup<RegisterModelForm>;

  constructor(
    private fb: FormBuilder,
     private identityService: IdentityService, 
     private router: Router) { }

  ngOnInit(): void {
    this.registerForm = this.fb.group<RegisterModelForm>({
      email: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required]
    })
  }

  register(): void {
    this.identityService.register(this.registerForm.value)
      .subscribe(res => {
        this.router.navigate(['somewhere']);
    })
  }

}
