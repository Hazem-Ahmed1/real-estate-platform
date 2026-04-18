import { ChangeDetectionStrategy, Component, output, signal } from '@angular/core';

@Component({
  selector: 'app-admin-navbar',
  imports: [],
  templateUrl: './admin-navbar.html',
  styleUrl: './admin-navbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminNavbar {
  readonly periods = signal(['آخر 7 أيام', 'آخر 30 يوم', 'آخر 90 يوم', 'هذا العام']);
  readonly selectedPeriod = signal(this.periods()[0]);

  readonly periodChange = output<string>();

  onPeriodChange(event: Event): void {
    const target = event.target;

    if (!(target instanceof HTMLSelectElement)) {
      return;
    }

    const period = target.value;
    this.selectedPeriod.set(period);
    this.periodChange.emit(period);
  }
}
