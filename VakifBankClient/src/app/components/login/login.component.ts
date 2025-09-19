// src/app/components/login/login.component.ts
import { Component } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/auth.model';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username = '';
  password = '';
  errorMessage: string | null = null;
  successMessage: string | null = null; // <-- kayıt sonrası başarı mesajı
  
  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    // ?registered=1 ile gelindiyse bilgilendir
    this.route.queryParamMap.subscribe(p => {
      if (p.get('registered') === '1') {
        this.successMessage = 'Kayıt başarılı! Şimdi giriş yapabilirsiniz.';
      }
    });
  }

  onSubmit(): void {
    this.errorMessage = null;
    this.successMessage = null;

    const loginRequest: LoginRequest = {
      username: this.username,
      password: this.password
    };

    this.authService.login(loginRequest).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.errorMessage = 'Kullanıcı adı veya şifre yanlış.';
      }
    });
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}
