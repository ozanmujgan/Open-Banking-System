// src/app/models/my-bank.model.ts

// MyBank API'sinden gelen hesap modeli
export interface MyBankAccount {
  id?: number;
  userId?: number;
  AccountNumber?: string;
  IBAN?: string;
  Balance?: number;
  CurrencyCode?: string;
  AccountName?: string;
  // Diğer olası alanlar
}

// MyBank API'sinden gelen işlem modeli
export interface MyBankTransaction {
  id?: number;
  bankAccountId?: number;
  amount?: number;
  transactionType?: string;
  description?: string;
  transactionDate?: Date;
  // Diğer olası alanlar
}

// MyBank API'sinden gelen kullanıcı modeli
export interface MyBankUser {
  id?: number;
  username?: string;
  passwordHash?: string;
  // Diğer olası alanlar
}