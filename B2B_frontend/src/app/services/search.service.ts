import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SearchService {

  private http = inject(HttpClient);

  private api = 'https://localhost:7033/api/Leads';

  search(keyword: string): Observable<any> {

    return this.http.post(
      `${this.api}/search-by-keyword?keyword=${encodeURIComponent(keyword)}`,
      {}
    );
    
  }
  getAllLeads() {

  return this.http.get<any[]>(
    "https://localhost:7033/api/Leads"
  );

}
getDashboard() {

  return this.http.get<any>(
    "https://localhost:7033/api/Leads/dashboard"
  );

}

deleteLead(id: number) {

  return this.http.delete(
    `${this.api}/${id}`
  );

}

updateLead(id: number, lead: any) {

  return this.http.put(

    `${this.api}/${id}`,

    lead

  );

}
exportExcel() {

  return this.http.get(
    `${this.api}/export`,
    {
      responseType: 'blob'
    }
  );

}
getRecentLeads() {

  return this.http.get<any[]>(
    `${this.api}/recent`
  );

}
}