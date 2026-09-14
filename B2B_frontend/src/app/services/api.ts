import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private http = inject(HttpClient);

  private api = 'https://localhost:7033/api';

  constructor() { }

  get(url: string) {

    return this.http.get(`${this.api}/${url}`);

  }

  post(url: string, body: any) {

    return this.http.post(`${this.api}/${url}`, body);

  }

  put(url: string, body: any) {

    return this.http.put(`${this.api}/${url}`, body);

  }

  delete(url: string) {

    return this.http.delete(`${this.api}/${url}`);

  }

}