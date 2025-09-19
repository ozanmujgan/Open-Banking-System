// src/app/services/my-bank-api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyBankAccount, MyBankTransaction } from '../models/my-bank.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MyBankApiService {
  private apiUrl = 'http://localhost:5231/api/mybank';

  constructor(private http: HttpClient, private authService: AuthService) {}

  getAccounts(): Observable<MyBankAccount[]> {
    const token = this.authService.getToken();
    if (!token) {
      throw new Error('Kullanıcı oturum açmamış veya token alınamadı.');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
    return this.http.get<MyBankAccount[]>(`${this.apiUrl}/accounts`, { headers });
  }

  getAccountDetail(accountNumber: string): Observable<MyBankAccount> {
    const token = this.authService.getToken();
    if (!token) {
      throw new Error('Kullanıcı oturum açmamış veya token alınamadı.');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
    return this.http.get<MyBankAccount>(`${this.apiUrl}/accounts/${accountNumber}`, { headers });
  }

  
  getTransactions(bankAccountId: number): Observable<MyBankTransaction[]> {
    const token = this.authService.getToken();
    if (!token) {
      throw new Error('Kullanıcı oturum açmamış veya token alınamadı.');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
    return this.http.get<MyBankTransaction[]>(`${this.apiUrl}/transactions/${bankAccountId}`, { headers });
  }
}