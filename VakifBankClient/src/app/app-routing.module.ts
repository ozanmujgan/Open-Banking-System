// src/app/app-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Gerekli tüm bileşenleri içe aktarın
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';

import { AccountListComponent } from './components/account-list/account-list.component';
import { AccountDetailComponent } from './components/account-detail/account-detail.component';
import { TransactionListComponent } from './components/transaction-list/transaction-list.component';
import { AuthGuard } from './guards/auth-guard';

const routes: Routes = [
  // Uygulama başladığında ilk yönlenecek sayfa.
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // Dashboard ana rotası, artık hiçbir child rotası yok.
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },

  
  
  { 
    path: 'account-list/:bank', 
    component: AccountListComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'account-detail/:bank/:id', 
    component: AccountDetailComponent, 
    canActivate: [AuthGuard] 
  },
  // BURADA DEĞİŞİKLİK YAPILDI: ':accountNumber' yerine ':id' kullanıldı
  { 
    path: 'transactions/:bank/:id', 
    component: TransactionListComponent, 
    canActivate: [AuthGuard] 
  },

  
  // Tanımsız tüm rotaları login sayfasına yönlendir.
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
