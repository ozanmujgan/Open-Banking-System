// src/app/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5231/api/mybank';

  private userSubject = new BehaviorSubject<User | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.loadUserFromLocalStorage();
  }

  // === REGISTER ===
  // RegisterComponent içinde this.auth.register(username, password).subscribe(...) diyerek kullan.
  register(username: string, password: string): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/register`, { username, password })
      .pipe(catchError(this.handleError));
  }

  // === LOGIN ===
  login(credentials: { username: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, credentials).pipe(
      tap((response: any) => {
        const user: User = {
          token: response.token,
          userId: response.userId,
          username: credentials.username,
          fullName: response.username || credentials.username
        };
        localStorage.setItem('currentUser', JSON.stringify(user));
        this.userSubject.next(user);
        this.router.navigate(['/dashboard']);
      }),
      catchError(this.handleError)
    );
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    this.userSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    const user = this.userSubject.getValue();
    return user ? user.token : null;
  }

  // İsteğe bağlı yardımcılar
  get currentUser(): User | null {
    return this.userSubject.getValue();
  }
  get isLoggedIn(): boolean {
    return !!this.getToken();
  }

  // === PRIVATE ===
  private loadUserFromLocalStorage(): void {
    const userData = localStorage.getItem('currentUser');
    if (!userData) return;
    try {
      const user: User = JSON.parse(userData);
      this.userSubject.next(user);
    } catch {
      localStorage.removeItem('currentUser');
    }
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Bilinmeyen bir hata oluştu.';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Bir hata oluştu: ${error.error.message}`;
    } else {
      if (error.status === 400 && error.error) {
        errorMessage = error.error.detail || error.error.message || 'Geçersiz istek.';
      } else if (error.status === 401) {
        errorMessage = 'Kullanıcı adı veya şifre yanlış.';
      } else {
        errorMessage = `Sunucu hatası: ${error.status} ${error.statusText || ''} - ${error.error?.message || error.message}`;
      }
    }
    console.error('AuthService hatası:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
