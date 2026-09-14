import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {

  email = '';

  password = '';

  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  login() {

    this.errorMessage = '';

    const data = {
      email: this.email,
      password: this.password
    };

    this.authService.login(data).subscribe({

      next: (res: any) => {

        alert("Login Successful");

        // JWT Token Save
        localStorage.setItem("token", res.token);

        // User Save
        localStorage.setItem("user", JSON.stringify(res.user));

        this.router.navigate(['/dashboard']);

      },

      error: (err: any) => {

        this.errorMessage = "Invalid Email or Password";

      }

    });

  }

}