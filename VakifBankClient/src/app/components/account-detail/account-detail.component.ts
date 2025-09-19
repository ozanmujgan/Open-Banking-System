import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service'; 
import { Subscription, Observable, of } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';

// MyBank'ten gelecek veri yapısı
interface MyBankAccountDetail {
  id: number;
  userId: number;
  accountNumber: string;
  iban: string;
  balance: number;
  currencyCode: string;
  accountName: string;
}

// Vakıfbank'tan gelecek accountInfo objesinin yapısı
interface VakifBankAccountInfo {
  currencyCode: string;
  lastTransactionDate: string;
  accountStatus: string;
  openingDate: string;
  iban: string;
  customerNumber: string;
  remainingBalance: string;
  balance: string;
  accountType: string;
  branchCode: string;
  accountNumber: string;
}

// API yanıtının tamamı
interface VakifBankApiResponse {
  header: any;
  data: {
    accountInfo: VakifBankAccountInfo;
  };
}

@Component({
  selector: 'app-account-detail',
  templateUrl: './account-detail.component.html',
  styleUrls: ['./account-detail.component.css'],
  standalone: false
})
export class AccountDetailComponent implements OnInit, OnDestroy {
  accountDetail: any | null = null;
  isLoading = true;
  errorMessage: string | null = null;
  bankName: string | null = null;

  private routeSub: Subscription | undefined;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private auth: AuthService ,
  ) { }

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.pipe(
      switchMap(params => {
        const bankNameFromRoute = params.get('bank');
        // Rota parametresinden gelen genel kimlik bilgisini ('id' olarak) alıyoruz.
        // Bu kimlik, bankaya göre IBAN veya hesap numarası olabilir.
        const identifierFromRoute = params.get('id');

        if (!bankNameFromRoute || !identifierFromRoute) {
          this.errorMessage = 'Banka adı veya hesap bilgisi eksik.';
          this.isLoading = false;
          return of(null);
        }

        this.bankName = bankNameFromRoute;
        this.isLoading = true;
        this.errorMessage = null;

        const apiBankName = (this.bankName || '').toLowerCase();

if (apiBankName === 'mybank' || apiBankName === 'mbank') {
  // MyBank DETAY endpoint'i IBAN bekliyor!
  return this.fetchMyBankDetail(identifierFromRoute!);
} else if (apiBankName === 'vakifbank') {
  return this.fetchVakifBankDetail(identifierFromRoute!);
} else {
  this.errorMessage = 'Desteklenmeyen banka adı.';
  this.isLoading = false;
  return of(null);
}
      })
    ).subscribe({
      next: (detail) => {
        this.isLoading = false;
        this.accountDetail = detail;
        if (!this.accountDetail) {
          this.errorMessage = 'Hesap detayları bulunamadı.';
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Hesap detayları yüklenirken bir hata oluştu.';
        console.error('API Error:', err);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
  }

  fetchVakifBankDetail(accountNumber: string): Observable<VakifBankAccountInfo | null> {
    const apiUrl = `http://localhost:5231/api/vakifbank/accountDetail`;
    const requestBody = { accountNumber };

    return this.http.post<VakifBankApiResponse>(apiUrl, requestBody).pipe(
      switchMap(response => {
        if (response?.data?.accountInfo) {
          return of(response.data.accountInfo);
        } else {
          this.errorMessage = 'Vakıfbank hesap bilgileri beklenilen formatta değil.';
          return of(null);
        }
      }),
      catchError(err => {
        this.errorMessage = 'Vakıfbank hesap detayları çekilirken bir hata oluştu.';
        return of(null);
      })
    );
  }
  isMyBank(): boolean {
  const b = (this.bankName || '').toUpperCase();
  return b === 'MBANK' || b === 'MYBANK';
}

  fetchMyBankDetail(iban: string): Observable<MyBankAccountDetail | null> {
  const apiUrl = `http://localhost:5231/api/mybank/AccountDetail/${encodeURIComponent(iban)}`;

  const token = this.auth.getToken();
  const headers = token
    ? new HttpHeaders({ Authorization: `Bearer ${token}` })
    : new HttpHeaders();

  return this.http.get<MyBankAccountDetail>(apiUrl, { headers }).pipe(
    switchMap(response => response ? of(response) : (this.errorMessage = 'MyBank hesap bilgileri beklenilen formatta değil.', of(null))),
    catchError(_ => { this.errorMessage = 'MyBank hesap detayları çekilirken bir hata oluştu.'; return of(null); })
  );
}
}
