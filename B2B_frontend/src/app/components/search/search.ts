import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { SearchService } from '../../services/search.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './search.html',
  styleUrl: './search.css'
})
export class SearchComponent {

  keyword: string = '';

  loading = false;

  result: any;

  constructor(private searchService: SearchService) {}

  search(): void {

    if (!this.keyword.trim()) {
      alert("Enter keyword");
      return;
    }

    this.loading = true;

    this.searchService.search(this.keyword).subscribe({

      next: (res: any) => {

        this.result = res;

        this.loading = false;

      },

      error: (err) => {

        console.log(err);

        this.loading = false;

      }

    });

  }

}