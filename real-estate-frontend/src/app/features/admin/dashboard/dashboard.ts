import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

type TrendDirection = 'up' | 'down' | 'flat';

interface KpiCard {
  label: string;
  value: string;
  trend: string;
  direction: TrendDirection;
  iconClass: string;
  badgeClass: string;
}

interface SourceItem {
  label: string;
  percent: string;
  colorClass: string;
}

interface TopPageItem {
  page: string;
  views: string;
  bounce: string;
  direction: TrendDirection;
}

interface RegionItem {
  flag: string;
  name: string;
  value: string;
  width: number;
  barClass: string;
}

interface ChartBar {
  height: number;
  colorClass: string;
}

@Component({
  selector: 'app-admin-dashboard',
  imports: [],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboard {
  readonly kpis = signal<KpiCard[]>([
    {
      label: 'مشاهدات الصفحات',
      value: '2.4M',
      trend: '18.2%',
      direction: 'up',
      iconClass: 'fa-solid fa-eye',
      badgeClass: 'kpi-badge kpi-badge-primary',
    },
    {
      label: 'الزوار الفريدون',
      value: '842K',
      trend: '12.5%',
      direction: 'up',
      iconClass: 'fa-solid fa-users',
      badgeClass: 'kpi-badge kpi-badge-blue',
    },
    {
      label: 'متوسط الجلسة',
      value: '4:32',
      trend: '8.1%',
      direction: 'up',
      iconClass: 'fa-solid fa-clock',
      badgeClass: 'kpi-badge kpi-badge-violet',
    },
    {
      label: 'معدل الارتداد',
      value: '32.1%',
      trend: '2.3%',
      direction: 'down',
      iconClass: 'fa-solid fa-percent',
      badgeClass: 'kpi-badge kpi-badge-amber',
    },
  ]);

  readonly chartBars = signal<ChartBar[]>([
    { height: 45, colorClass: 'bar-level-1' },
    { height: 55, colorClass: 'bar-level-2' },
    { height: 40, colorClass: 'bar-level-1' },
    { height: 70, colorClass: 'bar-level-3' },
    { height: 85, colorClass: 'bar-level-4' },
    { height: 80, colorClass: 'bar-level-4' },
    { height: 65, colorClass: 'bar-level-3' },
    { height: 55, colorClass: 'bar-level-2' },
    { height: 75, colorClass: 'bar-level-3' },
    { height: 90, colorClass: 'bar-level-4' },
    { height: 100, colorClass: 'bar-level-5' },
    { height: 88, colorClass: 'bar-level-4' },
    { height: 72, colorClass: 'bar-level-3' },
    { height: 60, colorClass: 'bar-level-2' },
  ]);

  readonly sources = signal<SourceItem[]>([
    { label: 'بحث عضوي', percent: '38%', colorClass: 'dot-primary' },
    { label: 'زيارات مباشرة', percent: '24%', colorClass: 'dot-blue' },
    { label: 'وسائل التواصل', percent: '16%', colorClass: 'dot-amber' },
    { label: 'إحالات', percent: '12%', colorClass: 'dot-violet' },
    { label: 'مصادر أخرى', percent: '10%', colorClass: 'dot-slate' },
  ]);

  readonly topPages = signal<TopPageItem[]>([
    { page: '/dashboard', views: '348,201', bounce: '12.4%', direction: 'up' },
    { page: '/pricing', views: '256,122', bounce: '28.1%', direction: 'up' },
    { page: '/features', views: '198,445', bounce: '22.7%', direction: 'flat' },
    { page: '/blog', views: '145,320', bounce: '45.2%', direction: 'down' },
    { page: '/signup', views: '98,740', bounce: '8.9%', direction: 'up' },
  ]);

  readonly regions = signal<RegionItem[]>([
    {
      flag: '🇺🇸',
      name: 'الولايات المتحدة',
      value: '312K',
      width: 37,
      barClass: 'region-bar-primary',
    },
    {
      flag: '🇬🇧',
      name: 'المملكة المتحدة',
      value: '156K',
      width: 19,
      barClass: 'region-bar-blue',
    },
    {
      flag: '🇩🇪',
      name: 'ألمانيا',
      value: '118K',
      width: 14,
      barClass: 'region-bar-amber',
    },
    {
      flag: '🇯🇵',
      name: 'اليابان',
      value: '95K',
      width: 11,
      barClass: 'region-bar-violet',
    },
    {
      flag: '🇧🇷',
      name: 'البرازيل',
      value: '78K',
      width: 9,
      barClass: 'region-bar-rose',
    },
  ]);

  trendTextClass(direction: TrendDirection): string {
    if (direction === 'up') {
      return 'trend-up';
    }

    if (direction === 'down') {
      return 'trend-down';
    }

    return 'trend-flat';
  }

  trendIconClass(direction: TrendDirection): string {
    if (direction === 'up') {
      return 'fa-solid fa-arrow-trend-up';
    }

    if (direction === 'down') {
      return 'fa-solid fa-arrow-trend-down';
    }

    return 'fa-solid fa-minus';
  }
}
