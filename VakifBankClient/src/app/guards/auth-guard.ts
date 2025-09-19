// src/app/guards/auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> {
    return this.authService.user$.pipe(
      take(1),
      map(user => {
        const isAuth = !!user; // !!user -> null veya undefined ise false, aksi halde true
        if (isAuth) {
          return true; // Kullanıcı giriş yapmışsa devam et
        }
        // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
        this.router.navigate(['/login']);
        return false;
      })
    );
  }
}