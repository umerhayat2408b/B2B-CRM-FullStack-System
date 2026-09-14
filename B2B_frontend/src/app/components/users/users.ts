import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { User } from '../../interfaces/user';
import { UserService } from '../../services/user';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './users.html',
  styleUrl: './users.css'
})
export class UsersComponent implements OnInit {

  users: User[] = [];

  user: User = {
    id: 0,
    name: '',
    email: ''
  };

  isEdit = false;

  constructor(private userService: UserService) {}

  ngOnInit(): void {

    this.loadUsers();

  }

  loadUsers() {

    this.userService.getUsers().subscribe({

      next: (res: User[]) => {

        this.users = res;

      },

      error: (err: any) => {

        console.log(err);

      }

    });

  }

  saveUser() {

    if (this.isEdit) {

      this.userService.updateUser(this.user).subscribe(() => {

        this.loadUsers();

        this.resetForm();

      });

    } else {

      this.userService.addUser(this.user).subscribe(() => {

        this.loadUsers();

        this.resetForm();

      });

    }

  }

  editUser(u: User) {

    this.user = { ...u };

    this.isEdit = true;

  }

  deleteUser(id: number) {

    if (!confirm("Delete this user?"))
      return;

    this.userService.deleteUser(id).subscribe(() => {

      this.loadUsers();

    });

  }

  resetForm() {

    this.user = {
      id: 0,
      name: '',
      email: ''
    };

    this.isEdit = false;

  }

}