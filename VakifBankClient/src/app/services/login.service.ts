// src/app/services/login.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoginService {

  private apiUrl = 'http://localhost:5231/api/mybank';

  constructor(private http: HttpClient) { }

  login(username: string, password: string): Observable<any> {
    const loginRequest = { username, password };
    return this.http.post<any>(`${this.apiUrl}/login`, loginRequest).pipe(
      // Login başarılı olduğunda token'ı localStorage'a kaydet
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
          // Token'ı aldıktan sonra kullanıcı bilgilerini de kaydet
          localStorage.setItem('userId', response.userId);
          localStorage.setItem('username', response.username);
        }
      })
    );
  }
}
