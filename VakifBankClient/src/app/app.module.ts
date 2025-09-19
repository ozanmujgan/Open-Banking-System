// src/app/app.module.ts
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module'; // RouterModule'ü AppRoutingModule üzerinden alıyoruz
import { TokenInterceptor } from './interceptors/token.interceptor';


// Bileşenleri içeri aktar
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { AccountListComponent } from './components/account-list/account-list.component'; // Yeni
import { AccountDetailComponent } from './components/account-detail/account-detail.component'; // Yeni
import { TransactionListComponent } from './components/transaction-list/transaction-list.component'; // Yeni
import { AuthGuard } from './guards/auth-guard';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    DashboardComponent, 
    AccountListComponent, // Yeni bileşen
    AccountDetailComponent, // Yeni bileşen
    TransactionListComponent // Yeni bileşen
  ],
  imports: [
    BrowserModule,
    AppRoutingModule, // RouterModule'ü buradan alıyoruz
    FormsModule,
    HttpClientModule,
    ReactiveFormsModule
  ],
  providers: [
    // Burada servisleriniz tanımlanır
    AuthGuard,
    // ÖNEMLİ: TokenInterceptor'ı HTTP_INTERCEPTORS token'ı ile buraya kaydediyoruz
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }