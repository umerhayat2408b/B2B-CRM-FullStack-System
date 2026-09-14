import { Component, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { SearchService } from '../../services/search.service';

Chart.register(...registerables);

@Component({
  selector: 'app-charts',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './charts.html',
  styleUrl: './charts.css'
})
export class ChartsComponent implements AfterViewInit {

  constructor(private api: SearchService) { }

  ngAfterViewInit(): void {

    this.api.getDashboard().subscribe({

      next: (stats: any) => {

        this.createBarChart(stats);
        this.createPieChart(stats);

      },

      error: (err) => {

        console.log(err);

      }

    });

  }

  createBarChart(stats: any) {

    new Chart("barChart", {

      type: 'bar',

      data: {

        labels: [
          'Leads',
          'Emails',
          'Phones',
          'Websites'
        ],

        datasets: [{
          label: 'Dashboard Statistics',
          data: [
            stats.totalLeads,
            stats.totalEmails,
            stats.totalPhones,
            stats.totalWebsites
          ]
        }]

      }

    });

  }

  createPieChart(stats: any) {

    new Chart("pieChart", {

      type: 'pie',

      data: {

        labels: [
          'Emails',
          'Phones',
          'Websites'
        ],

        datasets: [{
          data: [
            stats.totalEmails,
            stats.totalPhones,
            stats.totalWebsites
          ]
        }]

      }

    });

  }

}