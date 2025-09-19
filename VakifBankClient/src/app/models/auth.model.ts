// API’ye gidecek istek
export interface LoginRequest {
  username: string;
  password: string;
}

// API’den dönen cevap
export interface LoginResponse {
  token: string;
  username?: string;
  userId?: number;        // sayı olması daha doğal
  message?: string;
  expiresIn?: number;
}
