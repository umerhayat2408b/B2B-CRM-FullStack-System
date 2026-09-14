import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SearchService } from '../../services/search.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-leads',
  standalone: true,
  imports: [CommonModule,
     FormsModule
  ],
  templateUrl: './leads.html',
  styleUrl: './leads.css'


})
export class LeadsComponent implements OnInit {
  selectedLead: any = {};
  leads: any[] = [];
  loading = false;
  searchText = "";
  currentPage = 1;
  pageSize = 10;
  editLead(lead: any) {
   
  this.selectedLead = { ...lead };

}

saveLead() {

  this.api.updateLead(

    this.selectedLead.id,

    this.selectedLead

  ).subscribe({

    next: () => {

      alert("Lead Updated Successfully.");

      this.loadLeads();

    },

    error: (err: any) => {

      console.log(err);

    }

  });

}
exportExcel() {

  console.log("Export button clicked");

  this.api.exportExcel().subscribe({

    next: (data: Blob) => {

      console.log("Excel received:", data);

      const blob = new Blob(
        [data],
        {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        }
      );

      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');

      a.href = url;
      a.download = 'B2B_Leads.xlsx';

      document.body.appendChild(a);

      a.click();

      document.body.removeChild(a);

      window.URL.revokeObjectURL(url);

    },

    error: (err: any) => {

      console.error("EXPORT ERROR:", err);

      alert("Export Failed. Status: " + err.status);

    }

  });

}
  deleteLead(id: number) {

  if (!confirm('Are you sure you want to delete this lead?')) {
    return;
  }
  
  this.api.deleteLead(id).subscribe({

    next: () => {

      alert("Lead Deleted Successfully.");

      this.loadLeads();

    },

    error: (err: any) => {

      console.log(err);

      alert("Delete Failed.");

    }

  });

}
  constructor(private api: SearchService) {}

  ngOnInit(): void {
    this.loadLeads();
  }

  loadLeads() {

    this.loading = true;

    this.api.getAllLeads().subscribe({

      next: (res) => {

        this.leads = res;
        this.loading = false;

      },

      error: (err) => {

        console.log(err);
        this.loading = false;

      }

    });

  }
get filteredLeads() {

  let data = this.leads;

  if (this.searchText) {

    data = data.filter((x: any) =>

      x.name?.toLowerCase().includes(this.searchText.toLowerCase()) ||

      x.email?.toLowerCase().includes(this.searchText.toLowerCase()) ||

      x.phone?.includes(this.searchText)

    );

  }

  const start = (this.currentPage - 1) * this.pageSize;

  return data.slice(start, start + this.pageSize);

}
nextPage() {

  if ((this.currentPage * this.pageSize) < this.leads.length) {

    this.currentPage++;

  }

}

previousPage() {

  if (this.currentPage > 1) {

    this.currentPage--;

  }

}

}