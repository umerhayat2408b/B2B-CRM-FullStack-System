import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SearchService } from '../../services/search.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class SidebarComponent {

  constructor(private api: SearchService) {}

  exportExcel() {

    this.api.exportExcel().subscribe({

      next: (data: Blob) => {

        const url = window.URL.createObjectURL(data);

        const a = document.createElement('a');

        a.href = url;
        a.download = 'B2B_Leads.xlsx';

        document.body.appendChild(a);

        a.click();

        document.body.removeChild(a);

        window.URL.revokeObjectURL(url);

      },

      error: (err: any) => {

        console.error('Export Error:', err);

        alert('Export Failed. Status: ' + err.status);

      }

    });

  }

}