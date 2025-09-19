// src/app/components/dashboard/dashboard.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { MyBankApiService } from '../../services/my-bank-api.service'; // <---

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  fullName = '';
  private userSubscription: Subscription | null = null;
  isMainDashboard = true;

  // yeni kullanıcıda sadece "Yeni Hesap Ekle" göstersin
  showAddOnly = false; // <---

  constructor(
    private authService: AuthService,
    private router: Router,
    private mybankApi: MyBankApiService // <---
  ) {}

  ngOnInit(): void {
    this.userSubscription = this.authService.user$.subscribe(user => {
      if (user) {
        this.fullName = user.fullName;
        // kullanıcı değiştiğinde hesap durumunu kontrol et
        this.checkAccounts();
      } else {
        this.fullName = '';
        this.router.navigate(['/login']);
      }
    });

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.isMainDashboard = event.urlAfterRedirects === '/dashboard';
      });
  }

  private checkAccounts(): void {
    // MyBank tarafında hesap yoksa sadece "Yeni Hesap Ekle" göster
    this.mybankApi.getAccounts().subscribe({
      next: accounts => {
        this.showAddOnly = !accounts || accounts.length === 0;
      },
      error: _ => {
        // hata olursa varsayılan davranış: kartları göster
        this.showAddOnly = false;
      }
    });
  }

  ngOnDestroy(): void {
    if (this.userSubscription) {
      this.userSubscription.unsubscribe();
    }
  }

  logout(): void {
    this.authService.logout();
  }
}
