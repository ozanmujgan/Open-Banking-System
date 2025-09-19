import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, of } from 'rxjs';

// ---------- VAKIFBANK tarafı ----------
interface VakifTransaction {
  currencyCode: string;
  transactionType: string;      // '1' (Gelen/CR) | '2' (Giden/DR) veya 'CR' / 'DR'
  description: string;
  amount: string;
  transactionCode: string;
  balance: string;
  transactionName: string;
  transactionDate: string;      // ISO tarih-saat
  transactionId: string;
}
interface VakifResponse {
  header: any;
  data: { accountTransactions: VakifTransaction[] };
}

// ---------- MBANK tarafı ----------
interface MyBankTransaction {
  id: number;
  bankAccountId: number;
  amount: number;               // pozitif/negatif olabilir
  transactionType: string;      // "Yatırım" | "Çekim" vs.
  description: string;
  transactionDate: string;      // ISO tarih-saat
}

@Component({
  selector: 'app-transaction-list',
  templateUrl: './transaction-list.component.html',
  styleUrls: ['./transaction-list.component.css'],
  standalone: false
})
export class TransactionListComponent implements OnInit {
  bankName!: 'MBANK' | 'VAKIFBANK';
  id!: string; // route param

  // Görselde göstermek için
  accountNumber?: string;   // VAKIFBANK
  bankAccountId?: number;   // MBANK

  // Tarih aralığı (chip ile belirlenir)
  presetRanges = [
    { key: '1M', label: '1 Ay', months: 1 },
    { key: '3M', label: '3 Ay', months: 3 },
    { key: '6M', label: '6 Ay', months: 6 },
    { key: '1Y', label: '1 Yıl', months: 12 },
  ];
  activeRangeKey: string | null = '1M';

  // İçsel tarih aralığı
  private currentStart!: Date;
  private currentEnd!: Date;

  // Ekranda gösterilecek metin
  displayStart = '';
  displayEnd = '';

  isLoading = false;
  errorMessage: string | null = null;

  // Listeler
  vakifTxns: VakifTransaction[] = [];
  mybankTxns: MyBankTransaction[] = [];

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.bankName = (params.get('bank') || '').toUpperCase() as 'MBANK' | 'VAKIFBANK';
      this.id = params.get('id') || '';

      if (this.bankName === 'MBANK') {
        this.bankAccountId = Number(this.id);
        this.accountNumber = undefined;
      } else {
        this.accountNumber = this.id;
        this.bankAccountId = undefined;
      }

      // Varsayılan olarak 1 Ay
      this.applyRange('1M'); // içerde fetchTransactions çağrılır
    });
  }

  // ---- Yardımcılar ----
  private toYMD(d: Date): string {
    // yyyy-MM-dd
    return d.toISOString().slice(0, 10);
  }
  private fmtTR(d: Date): string {
    // dd.MM.yyyy
    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const yyyy = d.getFullYear();
    return `${dd}.${mm}.${yyyy}`;
  }

  // ---- Chip seçimi ----
  applyRange(rangeKey: string) {
    this.activeRangeKey = rangeKey;

    const def = this.presetRanges.find(r => r.key === rangeKey);
    if (!def) return;

    const today = new Date();
    const start = new Date(today);
    start.setHours(0, 0, 0, 0);
    start.setMonth(start.getMonth() - def.months);

    this.currentEnd = new Date(today); // şimdi
    this.currentEnd.setHours(23, 59, 59, 999);
    this.currentStart = start;

    this.displayStart = this.fmtTR(this.currentStart);
    this.displayEnd = this.fmtTR(this.currentEnd);

    this.fetchTransactions();
  }

  // ---- Veri çekme ----
  fetchTransactions(): void {
    this.errorMessage = null;
    this.isLoading = true;
    this.vakifTxns = [];
    this.mybankTxns = [];

    // VAKIFBANK: +03:00 ile
    const vbStart = `${this.toYMD(this.currentStart)}T00:00:00+03:00`;
    const vbEnd   = `${this.toYMD(this.currentEnd)}T23:59:59+03:00`;

    // MBANK: UTC/ISO
    const mbStart = new Date(`${this.toYMD(this.currentStart)}T00:00:00`).toISOString();
    const mbEnd   = new Date(`${this.toYMD(this.currentEnd)}T23:59:59`).toISOString();

    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    if (this.bankName === 'MBANK') {
      if (!this.bankAccountId || isNaN(this.bankAccountId)) {
        this.fail('Geçersiz MBANK hesap bilgisi.');
        return;
      }
      const body = {
        bankAccountId: this.bankAccountId,
        startDate: mbStart,
        endDate: mbEnd
      };
      this.http.post<MyBankTransaction[]>(
        'http://localhost:5231/api/mybank/transactions',
        body,
        { headers }
      ).pipe(
        catchError(err => {
          console.error('MBANK API error:', err);
          this.fail('Seçilen tarih aralığında hareket bulunamadı.');
          return of([] as MyBankTransaction[]);
        })
      ).subscribe(list => {
        this.mybankTxns = list ?? [];
        this.isLoading = false;
      });

    } else {
      if (!this.accountNumber) {
        this.fail('Geçersiz Vakıfbank hesap numarası.');
        return;
      }
      const body = {
        accountNumber: this.accountNumber,
        startDate: vbStart,
        endDate: vbEnd
      };
      this.http.post<VakifResponse>(
        'http://localhost:5231/api/vakifbank/transactions',
        body,
        { headers }
      ).pipe(
        catchError(err => {
          console.error('Vakıfbank API error:', err);
          this.fail('Seçilen tarih aralığında hareket bulunamadı.');
          return of({ data: { accountTransactions: [] } } as unknown as VakifResponse);
        })
      ).subscribe(res => {
        this.vakifTxns = res?.data?.accountTransactions ?? [];
        this.isLoading = false;
      });
    }
  }

  private fail(msg: string) {
    this.errorMessage = msg;
    this.isLoading = false;
  }
}
