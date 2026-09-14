import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SearchService } from '../../services/search.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {

  keyword: string = '';

  loading: boolean = false;

  result: any;

  stats: any;

  recentLeads:any[]=[];
  constructor(private searchService: SearchService) { }

  ngOnInit(): void {

    this.loadDashboard();
    this.loadRecentLeads();

  }
  loadRecentLeads(): void {

  this.searchService.getRecentLeads().subscribe({

    next:(res:any[])=>{

      this.recentLeads=res;

    },

    error:(err:any)=>{

      console.log(err);

    }

  });

}

  loadDashboard(): void {

    this.searchService.getDashboard().subscribe({

      next: (res: any) => {

        this.stats = res;

      },

      error: (err: any) => {

        console.log(err);

      }

    });

  }

  search(): void {

    if (!this.keyword.trim()) {
      return;
    }

    this.loading = true;

    this.searchService.search(this.keyword).subscribe({

      next: (res: any) => {

        this.result = res;

        this.loading = false;

        this.loadDashboard();
        this.loadRecentLeads();
      },

      error: (err: any) => {

        console.log(err);

        this.loading = false;

      }

    });

  }

}