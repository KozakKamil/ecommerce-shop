import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})

export class ProfileComponent implements OnInit {
  firstName = signal('');
  lastName = signal('');
  email = signal('');

  nameSuccess = signal('');
  nameError = signal('');
  savingName = signal(false);

  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  passwordSuccess = signal('');
  passwordError = signal('');
  savingPassword = signal(false);

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
      this.authService.getProfile().subscribe({
        next: (data) => {
          this.firstName.set(data.firstName);
          this.lastName.set(data.lastName);
          this.email.set(data.email);
        }
      });
  }

  saveName(): void {
    this.savingName.set(true);
    this.nameSuccess.set('');
    this.nameError.set('');
    this.authService.updateName(this.firstName(), this.lastName()).subscribe({
      next: (data) => {
        this.savingName.set(false);
        this.nameSuccess.set('Dane zostały zapisane');
        const stored = localStorage.getItem('user');
        if (stored) {
          const user = JSON.parse(stored);
          user.firstName = this.firstName();
          user.lastName = this.lastName();
          localStorage.setItem('user', JSON.stringify(user));
        }
      },
      error: (err) => {
        this.savingName.set(false);
        this.nameError.set(err.error.message || 'Błąd podczas zapisywania danych');
      }
    });
  }

  changePassword(): void {
    if(this.newPassword !== this.confirmPassword){
      this.passwordError.set('Hasła nie są identyczne');
      return;
    }
    this.savingPassword.set(true);
    this.passwordSuccess.set('');
    this.passwordError.set('');
    this.authService.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: (data) => {
        this.savingPassword.set(false);
        this.passwordSuccess.set('Hasło zostało zmienione');
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
      },
      error: (err) => {
        this.savingPassword.set(false);
        const errors = Array.isArray(err.error) ? err.error.join(', ') : (err.error || 'Błąd podczas zmiany hasła');
        this.passwordError.set(errors);
      }
    });2
  }
}
