import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AdminMessageDetailsDto, AdminMessageListDto } from '../../../models/AdminMessageDto';
import { MessageService } from '../../../services/api/message.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-admin-messages',
  imports: [DatePipe],
  templateUrl: './admin-messages.html',
  styleUrl: './admin-messages.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminMessages implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly messageService = inject(MessageService);
  private readonly snackbar = inject(SnackbarService);

  readonly isLoading = signal(false);
  readonly isDeleting = signal(false);
  readonly errorMessage = signal('');
  readonly messages = signal<AdminMessageListDto[]>([]);
  readonly selectedMessage = signal<AdminMessageDetailsDto | null>(null);

  ngOnInit(): void {
    this.loadMessages();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMessages(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.messageService
      .getMessages({ page: 1, pageSize: 20 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.messages.set(response.items ?? []);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.getMessageError(error, 'load'));
          this.isLoading.set(false);
        },
      });
  }

  selectMessage(message: AdminMessageListDto): void {
    this.messageService
      .getMessage(message.messageId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (details) => {
          this.selectedMessage.set(details);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.getMessageError(error, 'load'));
          this.snackbar.show(this.errorMessage(), 'error');
        },
      });
  }

  clearSelection(): void {
    this.selectedMessage.set(null);
  }

  deleteMessage(message: AdminMessageListDto): void {
    this.isDeleting.set(true);
    this.errorMessage.set('');

    this.messageService
      .deleteMessage(message.messageId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messages.set(this.messages().filter((item) => item.messageId !== message.messageId));
          if (this.selectedMessage()?.messageId === message.messageId) {
            this.clearSelection();
          }
          this.isDeleting.set(false);
          this.snackbar.show('تم حذف الرسالة.', 'success');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.getMessageError(error, 'delete'));
          this.isDeleting.set(false);
          this.snackbar.show(this.errorMessage(), 'error');
        },
      });
  }

  private getMessageError(error: unknown, action: 'load' | 'delete'): string {
    const actionLabel = action === 'load' ? 'تحميل' : 'حذف';

    if (!(error instanceof HttpErrorResponse)) {
      return `تعذر ${actionLabel} الرسائل بسبب خطأ غير متوقع.`;
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

    return `تعذر ${actionLabel} الرسائل. حاول مرة أخرى.`;
  }
}
