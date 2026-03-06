import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';
import { RegisterDto } from '../../models/auth.model';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  registerDto: RegisterDto = {
    email: '',
    password: '',
    firstName: '',
    lastName: ''
  };
  error = '';
  loading = false;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    this.loading = true;
    this.error = '';

    this.authService.register(this.registerDto).subscribe({
      next: () => {
        this.router.navigate(['/products']);
      },
      error: (err) => {
        if (err.error && Array.isArray(err.error)) {
          this.error = err.error.map((e: any) => e.description).join(', ');
        } else {
          this.error = 'Błąd rejestracji. Spróbuj ponownie.';
        }
        this.loading = false;
      }
    });
  }
}