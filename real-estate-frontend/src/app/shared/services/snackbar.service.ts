import { Injectable, signal } from '@angular/core';

export type SnackbarType = 'success' | 'error' | 'info';

export interface SnackbarMessage {
  text: string;
  type: SnackbarType;
}

@Injectable({ providedIn: 'root' })
export class SnackbarService {
  readonly message = signal<SnackbarMessage | null>(null);

  /**
   * Show a snackbar message.
   * @param text Message text.
   * @param type Message type (default: 'info').
   * @param durationMs Auto‑hide after this many ms (default 2600).
   */
  show(text: string, type: SnackbarType = 'info', durationMs = 2600): void {
    this.message.set({ text, type });
    window.setTimeout(() => {
      if (this.message()?.text === text) {
        this.message.set(null);
      }
    }, durationMs);
  }

  /** Shortcut for success messages */
  success(text: string, durationMs = 2600): void {
    this.show(text, 'success', durationMs);
  }

  /** Shortcut for error messages */
  error(text: string, durationMs = 2600): void {
    this.show(text, 'error', durationMs);
  }

  /** Shortcut for info messages */
  info(text: string, durationMs = 2600): void {
    this.show(text, 'info', durationMs);
  }

  /** Clear current message */
  clear(): void {
    this.message.set(null);
  }
}

