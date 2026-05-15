import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnDestroy, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../../services/api/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login implements OnDestroy {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly destroy$ = new Subject<void>();

  readonly isSubmitting = signal(false);
  readonly hasTriedSubmit = signal(false);
  readonly authError = signal('');

  readonly loginForm = this.fb.group({
    email: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rememberMe: [false],
  });

  isControlInvalid(controlName: 'email' | 'password'): boolean {
    const control = this.loginForm.controls[controlName];
    return control.invalid && (control.touched || this.hasTriedSubmit());
  }

  continueWithGoogle(): void {
    if (typeof window !== 'undefined') {
      window.location.assign('https://accounts.google.com/signin');
    }
  }

  submit(): void {
    this.hasTriedSubmit.set(true);
    this.loginForm.markAllAsTouched();
    this.authError.set('');

    if (this.loginForm.invalid) {
      return;
    }

    const { email, password, rememberMe } = this.loginForm.getRawValue();
    const userName = email.trim();

    this.isSubmitting.set(true);
    this.authService
      .login({ userName, password }, rememberMe)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.loginForm.reset({ email: '', password: '', rememberMe: false });
          this.hasTriedSubmit.set(false);
          this.router.navigate(['/admin']);
        },
        error: (error: unknown) => {
          this.isSubmitting.set(false);
          this.authError.set(this.getLoginErrorMessage(error));
        },
      });
  }

  private getLoginErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'حدث خطأ غير متوقع. حاول مرة أخرى.';
    }

    if (error.status === 0) {
      return 'تعذر الاتصال بالخادم. تأكد أن السيرفر يعمل.';
    }

    if (error.status === 401 || error.status === 403) {
      return 'بيانات الدخول غير صحيحة. تحقق من اسم المستخدم وكلمة المرور.';
    }

    if (error.status >= 500) {
      return 'حدث خطأ في الخادم. حاول مرة أخرى لاحقاً.';
    }

    return 'تعذر تسجيل الدخول. تحقق من البيانات وحاول مرة أخرى.';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
