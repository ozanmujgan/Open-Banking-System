// src/app/models/account.model.ts

// VakıfBank API'sinden gelen hesap yanıtının yapısı
export interface VakifBankAccountResponse {
  Header?: {
    StatusCode?: string;
    StatusDescription?: string;
  };
  Data?: {
    Accounts?: Accounts[];
  };
}

// VakıfBank hesabı için model
export interface Accounts {
[x: string]: any;
  AccountNumber?: string;
  IBAN?: string;
  Balance?: number;
  CurrencyCode?: string;
  AccountName?: string;
}

// VakıfBank hesap detayı için model (ileride kullanılabilir)
export interface VakifBankAccountDetailResponse {
  Header?: {
    StatusCode?: string;
    StatusDescription?: string;
  };
  Data?: {
    AccountInfo?: AccountInfo;
  };
}

export interface AccountInfo {
  AccountNumber?: string;
  IBAN?: string;
  Balance?: number;
  CurrencyCode?: string;
  AccountStatus?: string;
  CustomerNumber?: string;
  BranchCode?: string;
  // Diğer detaylar
}

// VakıfBank işlem hareketleri için model (ileride kullanılabilir)
export interface VakifBankAccountTransactionsResponse {
  Header?: {
    StatusCode?: string;
    StatusDescription?: string;
  };
  Data?: {
    AccountTransactions?: AccountTransaction[];
  };
}

export interface AccountTransaction {
  TransactionDate?: string;
  Amount?: number;
  CurrencyCode?: string;
  Description?: string;
  TransactionType?: string;
  // Diğer işlem detayları
}

// VakıfBank API'sine gönderilen istek modelleri (örnekler)
export interface VakifBankAccountDetailRequest {
    AccountNumber?: string;
}

export interface VakifBankAccountTransactionsRequest {
    AccountNumber?: string;
    StartDate?: string;
    EndDate?: string;
}