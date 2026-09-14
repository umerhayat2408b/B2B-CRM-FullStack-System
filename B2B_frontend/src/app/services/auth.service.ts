import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);

  private api = "https://localhost:7033/api/Auth";

  login(data: any) {
    return this.http.post(`${this.api}/login`, data);
  }

  logout() {
    localStorage.removeItem("user");
    localStorage.removeItem("token");
  }

  saveUser(user: any) {
    localStorage.setItem("user", JSON.stringify(user));
   }

  saveToken(token: string) {
    localStorage.setItem("token", token);
  }

  getUser() {
    return JSON.parse(localStorage.getItem("user") || "null");
  }

  getToken() {
    return localStorage.getItem("token");
  }

  isLoggedIn() {
    return this.getToken() != null;
  }
}