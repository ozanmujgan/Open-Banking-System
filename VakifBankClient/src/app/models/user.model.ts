export interface User {
  token: string;
  userId: number;
  username: string;
  fullName: string; // DashboardComponent'te kullandığınız fullName için
}