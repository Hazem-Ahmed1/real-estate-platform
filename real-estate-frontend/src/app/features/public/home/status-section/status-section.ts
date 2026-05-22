import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { DashboardService } from '../../../../services/api/dashboard.service';

@Component({
  selector: 'app-status-section',
  templateUrl: './status-section.html',
  styleUrl: './status-section.css',
})
export class StatusSection implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private sub?: Subscription;

  readonly totalProjects = signal('0');
  readonly totalBuildings = signal('0');
  readonly totalUnits = signal('0');
  readonly hasLoaded = signal(false);

  ngOnInit(): void {
    this.sub = this.dashboardService.getPublicStats().subscribe({
      next: (data) => {
        this.totalProjects.set(data.totalProjects.toLocaleString());
        this.totalBuildings.set(data.totalBuildings.toLocaleString());
        this.totalUnits.set(data.totalUnits.toLocaleString());
        this.hasLoaded.set(true);
      },
      error: () => this.hasLoaded.set(true),
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
