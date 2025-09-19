import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';


interface VakifBankAccountResponse {
  header: any;
  data: { accounts: any[] };
}
// MyBank için basit bir interface
interface MyBankAccount {
  id: number;
  userId: number;
  accountNumber: string;
  iban: string;
  balance: number;
  currencyCode: string;
  accountName: string;
}


@Component({
  selector: 'app-account-list',
  standalone: false,
  templateUrl: './account-list.component.html',
  styleUrls: ['./account-list.component.css'],
 
})
export class AccountListComponent implements OnInit {
  accounts: any[] = [];
  errorMessage: string | null = null;
  isLoading = true;
  bankName: string | null = null;

  // UI ekleri
  query = '';

    private routeSub: Subscription | undefined; // Bellek sızıntısını önlemek için


  constructor(private http: HttpClient, private route: ActivatedRoute) {} // 'route'u ekleyin

   ngOnInit(): void {
    // Route parametresindeki değişiklikleri dinle
    this.routeSub = this.route.paramMap.subscribe(params => {
      this.bankName = params.get('bank');
      if (this.bankName) {
        this.fetchAccounts(this.bankName);
      }
    });
  }

  ngOnDestroy(): void {
    // Aboneliği sonlandır
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
  }


  get filteredAccounts() {
    const q = (this.query || '').toLowerCase().trim();
    if (!q) return this.accounts;
    return this.accounts.filter(a =>
      (a.iban || '').toLowerCase().includes(q) ||
      (a.accountNumber || '').toLowerCase().includes(q)
    );
  }

  fetchAccounts(bankName: string): void {
    this.isLoading = true;
    this.errorMessage = null;
    let apiUrl: string;
    let isVakifbank = false;

    if (bankName.toLowerCase() === 'vakifbank') {
      apiUrl = 'http://localhost:5231/api/vakifbank/accounts';
      isVakifbank = true;
    } else if (bankName.toLowerCase() === 'mbank') {
      apiUrl = 'http://localhost:5231/api/mybank/accounts';
      isVakifbank = false;
    } else {
      this.errorMessage = 'Desteklenmeyen banka adı.';
      this.isLoading = false;
      return;
    }

    // Vakıfbank için POST, MyBank için GET kullanıyoruz.
    const request: import('rxjs').Observable<any> = isVakifbank
      ? this.http.post<VakifBankAccountResponse>(apiUrl, {})
      : this.http.get<MyBankAccount[]>(apiUrl);

    request.subscribe({
      next: (response: any) => {
        this.isLoading = false;
        if (isVakifbank) {
          if (response?.data?.accounts) {
            this.accounts = response.data.accounts;
          } else {
            this.errorMessage = 'Vakıfbank hesap bilgileri beklenilen formatta değil.';
          }
        } else {
          // MyBank'in doğrudan bir dizi döndürdüğünü varsayıyoruz
          this.accounts = response;
        }

        if (this.accounts.length === 0) {
          this.errorMessage = 'Görüntülenecek hesap bulunamadı.';
        }
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 401) {
          this.errorMessage = 'Yetkilendirme hatası. Lütfen tekrar giriş yapın.';
        } else {
          this.errorMessage = err.message || 'Bir hata oluştu, lütfen daha sonra tekrar deneyin.';
        }
      }
    });
  }

  async copy(text: string) {
    try {
      await navigator.clipboard.writeText(text);
    } catch {
      // no-op
    }
  }
}
