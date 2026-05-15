import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-snackbar',
  imports: [NgClass],
  templateUrl: './snackbar.html',
  styleUrl: './snackbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Snackbar {
  private readonly snackbarService = inject(SnackbarService);

  readonly message = computed(() => this.snackbarService.message());

  dismiss(): void {
    this.snackbarService.clear();
  }
}
