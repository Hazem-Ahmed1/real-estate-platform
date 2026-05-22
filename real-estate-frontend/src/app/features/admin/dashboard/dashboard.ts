import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DashboardService } from '../../../services/api/dashboard.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe, DecimalPipe } from '@angular/common';

type TrendDirection = 'up' | 'down' | 'flat';

@Component({
  selector: 'app-admin-dashboard',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboard implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly dashboardService = inject(DashboardService);
  private readonly snackbar = inject(SnackbarService);

  readonly isLoading = signal(false);
  readonly errorMessage = signal('');
  
  readonly stats = signal<any>(null);
  readonly chartData = signal<any>(null);

  // Chart filter signal
  readonly chartFilter = signal<'thisYear' | 'prevYear' | 'all'>('thisYear');

  // Load chart data with optional filter
  private loadChart(filter: 'thisYear' | 'prevYear' | 'all' = 'thisYear'): void {
    this.dashboardService.getAdminChart(filter).subscribe({
      next: (chart) => {
        this.chartData.set(chart);
      },
      error: (error) => {
        this.snackbar.show(this.getErrorMessage(error), 'error');
      },
    });
  }

  // Set chart filter from UI
  setChartFilter(filter: 'thisYear' | 'prevYear' | 'all'): void {
    this.chartFilter.set(filter);
    this.loadChart(filter);
  }

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    forkJoin({
      stats: this.dashboardService.getAdminStats(),
      chart: this.dashboardService.getAdminChart('thisYear')
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (response) => {
        this.stats.set(response.stats);
        this.chartData.set(response.chart);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.isLoading.set(false);
        this.snackbar.show(this.errorMessage(), 'error');
      }
    });
  }

  getMaxChartValue(): number {
    const data = this.chartData()?.datasets[0]?.data;
    if (!data || data.length === 0) return 10;
    const max = Math.max(...data);
    // Return the next multiple of 5 above the max for better scaling
    return max > 0 ? Math.ceil(max / 5) * 5 : 10;
  }

  getBarHeight(value: number, max: number): number {
    if (!max || max === 0) return 0;
    return Math.round((value / max) * 100);
  }

  getBarColorClass(value: number, max: number): string {
    const percentage = this.getBarHeight(value, max);
    if (percentage > 80) return 'bar-level-5';
    if (percentage > 60) return 'bar-level-4';
    if (percentage > 40) return 'bar-level-3';
    if (percentage > 20) return 'bar-level-2';
    return 'bar-level-1';
  }

  getPieColorClass(index: number): string {
    const colors = ['dot-primary', 'dot-blue', 'dot-amber', 'dot-violet', 'dot-rose', 'dot-slate'];
    return colors[index % colors.length];
  }

  getPieColorCode(index: number): string {
    const colors = ['var(--admin-primary-500)', 'var(--admin-blue-500)', 'var(--admin-amber-500)', 'var(--admin-violet-500)', 'var(--admin-rose-500)'];
    return colors[index % colors.length];
  }

  getDonutGradient(): string {
    const statuses = this.stats()?.pieCharts?.unitStatuses || [];
    const total = statuses.reduce((sum: number, item: any) => sum + item.count, 0);
    
    if (total === 0) return 'conic-gradient(var(--admin-border-soft) 0% 100%)';

    let currentPercent = 0;
    const gradients = statuses.map((status: any, index: number) => {
      const percentage = (status.count / total) * 100;
      const start = currentPercent;
      currentPercent += percentage;
      const end = currentPercent;
      return `${this.getPieColorCode(index)} ${start}% ${end}%`;
    });

    return `conic-gradient(${gradients.join(', ')})`;
  }

  getRegionColorClass(index: number): string {
    const colors = ['region-bar-primary', 'region-bar-blue', 'region-bar-amber', 'region-bar-violet', 'region-bar-rose'];
    return colors[index % colors.length];
  }

  trendTextClass(value: number): string {
    if (value > 50) return 'trend-up';
    if (value < 20) return 'trend-down';
    return 'trend-flat';
  }

  trendIconClass(value: number): string {
    if (value > 50) return 'fa-solid fa-arrow-trend-up';
    if (value < 20) return 'fa-solid fa-arrow-trend-down';
    return 'fa-solid fa-minus';
  }

  private getErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'تعذر تحميل الإحصائيات بسبب خطأ غير متوقع.';
    }

    if (error.status === 0) {
      return 'تعذر الاتصال بالخادم. تأكد أن السيرفر يعمل.';
    }

    if (error.status === 401 || error.status === 403) {
      return 'ليست لديك صلاحية. سجل الدخول بحساب إداري.';
    }

    if (error.status >= 500) {
      return 'حدث خطأ في الخادم. حاول مرة أخرى لاحقاً.';
    }

    return 'تعذر تحميل الإحصائيات. حاول مرة أخرى.';
  }
}
