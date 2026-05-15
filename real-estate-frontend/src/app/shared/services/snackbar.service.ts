import { Injectable, signal } from '@angular/core';

export type SnackbarType = 'success' | 'error' | 'info';

export interface SnackbarMessage {
  text: string;
  type: SnackbarType;
}

@Injectable({ providedIn: 'root' })
export class SnackbarService {
  readonly message = signal<SnackbarMessage | null>(null);

  show(text: string, type: SnackbarType = 'info', durationMs = 2600): void {
    this.message.set({ text, type });

    window.setTimeout(() => {
      if (this.message()?.text === text) {
        this.message.set(null);
      }
    }, durationMs);
  }

  clear(): void {
    this.message.set(null);
  }
}
