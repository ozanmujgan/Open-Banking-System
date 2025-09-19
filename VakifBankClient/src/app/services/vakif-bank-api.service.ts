// src/app/services/vakif-bank-api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

// Backend'den beklediğimiz veri modellerini burada tanımlıyoruz
interface VakifBankAccountResponse {
  Header: any;
  Data: {
    Accounts: any[];
  };
}

interface VakifBankAccountDetailRequest {
  data: {
    accountNumber: string;
  };
}

interface VakifBankAccountDetailResponse {
  Header: any;
  Data: {
    AccountInfo: any;
  };
}

interface VakifBankAccountTransactionsRequest {
  data: {
    accountNumber: string;
    startDate: string;
    endDate: string;
  };
}

interface VakifBankAccountTransactionsResponse {
  Header: any;
  Data: {
    AccountTransactions: any[];
  };
}

@Injectable({
  providedIn: 'root'
})
export class VakifBankApiService {
  private apiUrl = 'http://localhost:5231/api/vakifbank';

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwtToken'); // Token'ı localStorage'dan alıyoruz
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}` // 'Bearer' şemasıyla token'ı ekliyoruz
    });
  }

  /**
   * Hesap listesini çekmek için backend'in POST endpoint'ini çağırır.
   */
  getAccounts(): Observable<VakifBankAccountResponse> {
    const headers = this.getAuthHeaders();
    return this.http.post<VakifBankAccountResponse>(`${this.apiUrl}/accounts`, {}, { headers });
  }

  /**
   * Hesap detaylarını çekmek için backend'in POST endpoint'ini çağırır.
   */
  getAccountDetail(requestBody: VakifBankAccountDetailRequest): Observable<VakifBankAccountDetailResponse> {
    const headers = this.getAuthHeaders();
    return this.http.post<VakifBankAccountDetailResponse>(`${this.apiUrl}/accountDetail`, requestBody, { headers });
  }

  /**
   * Hesap hareketlerini çekmek için backend'in POST endpoint'ini çağırır.
   */
  getAccountTransactions(requestBody: VakifBankAccountTransactionsRequest): Observable<VakifBankAccountTransactionsResponse> {
    const headers = this.getAuthHeaders();
    return this.http.post<VakifBankAccountTransactionsResponse>(`${this.apiUrl}/transactions`, requestBody, { headers });
  }
}
