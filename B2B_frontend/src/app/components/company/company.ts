import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CompanyService } from '../../services/company.service';
import { Company } from '../../interfaces/company';

@Component({
  selector: 'app-company',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './company.html',
  styleUrl: './company.css'
})
export class CompanyComponent implements OnInit {

  companies: Company[] = [];

  company: Company = {
    id: 0,
    name: '',
    companyname: '',
    email: '',
    createdAt: ''
  };

  isEdit = false;

  constructor(private companyService: CompanyService) { }

  ngOnInit(): void {

    this.loadCompanies();

  }

  loadCompanies() {

    this.companyService.getCompanies().subscribe({

      next: (res: Company[]) => {

        this.companies = res;

      },

      error: (err: any) => {

        console.log(err);

      }

    });

  }

  saveCompany() {

    if (this.isEdit) {

      this.companyService.updateCompany(this.company).subscribe(() => {

        this.loadCompanies();

        this.resetForm();

      });

    }
    else {

      this.companyService.addCompany(this.company).subscribe(() => {

        this.loadCompanies();

        this.resetForm();

      });

    }

  }

  editCompany(c: Company) {

    this.company = { ...c };

    this.isEdit = true;

  }

  deleteCompany(id: number) {

    if (!confirm("Delete this company?"))
      return;

    this.companyService.deleteCompany(id).subscribe(() => {

      this.loadCompanies();

    });

  }

  resetForm() {

    this.company = {
      id: 0,
      name: '',
      companyname: '',
      email: '',
      createdAt: ''
    };

    this.isEdit = false;

  }

}