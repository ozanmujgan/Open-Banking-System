// src/app/interceptors/token.interceptor.ts
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {

  constructor() {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const currentUserString = localStorage.getItem('currentUser');
    
    let token: string | null = null;

    if (currentUserString) {
      try {
        // localStorage'daki JSON stringini parse ediyoruz.
        const currentUser = JSON.parse(currentUserString);
        // Doğru anahtarı kullanarak sadece JWT'yi alıyoruz.
        token = currentUser.token; 
      } catch (e) {
        console.error('Hata: localStorage\'daki currentUser verisi parse edilemedi.', e);
      }
    }

    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      console.log('Token eklendi:', request.headers.get('Authorization'));
    } else {
      console.log('Token bulunamadı veya geçersiz. İstek tokensız devam ediyor.');
    }

    return next.handle(request);
  }
}
