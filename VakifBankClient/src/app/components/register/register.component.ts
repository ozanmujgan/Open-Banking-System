// src/app/components/register/register.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: false
})
export class RegisterComponent {
  username = '';
  password = '';
  confirm = '';
  isLoading = false;
  error: string | null = null;
  info: string | null = null;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = null;
    if (!this.username || !this.password) {
      this.error = 'Kullanıcı adı ve şifre zorunludur.';
      return;
    }
    if (this.password.length !== 6) {
      this.error = 'Şifre tam 6 karakter olmalı.';
      return;
    }
    if (this.password !== this.confirm) {
      this.error = 'Şifreler uyuşmuyor.';
      return;
    }

    this.isLoading = true;
    this.auth.register(this.username, this.password).subscribe({
      next: () => {
        this.isLoading = false;
        // Başarılı kayıt sonrası login sayfasına gönder
        this.router.navigate(['/login'], {
          queryParams: { registered: '1' }
        });
      },
      error: (err) => {
        this.isLoading = false;
        this.error =
          err?.error ?? 'Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.';
      }
    });
  }
}
