import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Company } from '../interfaces/company';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {

  private http = inject(HttpClient);

  private api = 'https://localhost:7033/api/Company';

  constructor() { }

  // Get All Companies
  getCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(this.api);
  }

  // Get Company By Id
  getCompany(id: number): Observable<Company> {
    return this.http.get<Company>(`${this.api}/${id}`);
  }

  // Add Company
  addCompany(company: Company) {
    return this.http.post(this.api, company);
  }

  // Update Company
  updateCompany(company: Company) {
    return this.http.put(`${this.api}/${company.id}`, company);
  }

  // Delete Company
  deleteCompany(id: number) {
    return this.http.delete(`${this.api}/${id}`);
  }

}