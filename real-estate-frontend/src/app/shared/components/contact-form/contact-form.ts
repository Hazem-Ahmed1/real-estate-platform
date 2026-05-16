import { Component, inject, signal } from '@angular/core';
import { MessageService } from '../../../services/api/message.service';
import { PublicMessageCreateDto } from '../../../models/PublicMessageDto';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-contact-form',
  imports: [],
  templateUrl: './contact-form.html',
  styleUrl: './contact-form.css',
})
export class ContactForm {
  private readonly messageService = inject(MessageService);
  private readonly snackbar = inject(SnackbarService);

  readonly isSubmitting = signal(false);
  readonly statusMessage = signal('');

  form = signal({
    firstName: '',
    email: '',
    phone: '',
    subject: '',
    message: ''
  });

  readonly errors = signal({
    firstName: '',
    email: '',
    phone: '',
    subject: '',
    message: '',
  });

  updateField(field: string, value: string) {
    this.form.update(f => ({ ...f, [field]: value }));
    this.validateField(field, value);
  }

  submit() {
    const value = this.form();
    this.statusMessage.set('');

    const payload: PublicMessageCreateDto = {
      type: 'GeneralContact',
      fullName: value.firstName.trim(),
      email: value.email.trim(),
      phone: value.phone.trim(),
      subject: value.subject.trim(),
      messageBody: value.message.trim(),
    };

    if (!this.validateForm(payload)) {
      this.statusMessage.set('يرجى تعبئة جميع الحقول المطلوبة.');
      return;
    }

    this.isSubmitting.set(true);
    this.messageService.createMessage(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.form.set({
          firstName: '',
          email: '',
          phone: '',
          subject: '',
          message: '',
        });
        this.errors.set({
          firstName: '',
          email: '',
          phone: '',
          subject: '',
          message: '',
        });
        this.statusMessage.set('تم إرسال رسالتك بنجاح.');
        this.snackbar.show('تم إرسال رسالتك بنجاح.', 'success');
      },
      error: () => {
        this.isSubmitting.set(false);
        this.statusMessage.set('تعذر إرسال الرسالة. حاول مرة أخرى.');
        this.snackbar.show('تعذر إرسال الرسالة. حاول مرة أخرى.', 'error');
      },
    });
  }

  private validateForm(payload: PublicMessageCreateDto): boolean {
    const checks = {
      firstName: payload.fullName,
      email: payload.email,
      phone: payload.phone,
      subject: payload.subject,
      message: payload.messageBody,
    };

    Object.entries(checks).forEach(([key, value]) => {
      this.validateField(key, value);
    });

    const current = this.errors();
    return !current.firstName && !current.email && !current.phone && !current.subject && !current.message;
  }

  private validateField(field: string, value: string): void {
    const trimmed = value.trim();
    let error = '';

    if (!trimmed) {
      error = 'هذا الحقل مطلوب.';
    } else if (field === 'email') {
      const emailOk = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed);
      if (!emailOk) {
        error = 'يرجى إدخال بريد إلكتروني صحيح.';
      }
    } else if (field === 'phone') {
      const phoneOk = /^\+?[0-9]{8,15}$/.test(trimmed);
      if (!phoneOk) {
        error = 'يرجى إدخال رقم هاتف صحيح.';
      }
    }

    this.errors.update((current) => ({
      ...current,
      [field]: error,
    }));
  }
}
