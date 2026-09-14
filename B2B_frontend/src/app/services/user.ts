import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../interfaces/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private http = inject(HttpClient);

  private api = "https://localhost:7033/api/User";

  getUsers():Observable<User[]>{

    return this.http.get<User[]>(this.api);

  }

  addUser(user:User){

    return this.http.post(this.api,user);

  }

  updateUser(user:User){

    return this.http.put(`${this.api}/${user.id}`,user);

  }

  deleteUser(id:number){

    return this.http.delete(`${this.api}/${id}`);

  }

}