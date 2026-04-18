import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Register {
  private readonly fb = inject(NonNullableFormBuilder);

  readonly isSubmitting = signal(false);
  readonly hasTriedSubmit = signal(false);

  readonly registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(/^(?:\+966|0)?5\d{8}$/)]],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/),
      ],
    ],
    confirmPassword: ['', [Validators.required]],
    termsAccepted: [false, [Validators.requiredTrue]],
  });

  isControlInvalid(
    controlName:
      | 'fullName'
      | 'email'
      | 'phone'
      | 'password'
      | 'confirmPassword'
      | 'termsAccepted'
  ): boolean {
    const control = this.registerForm.controls[controlName];
    return control.invalid && (control.touched || this.hasTriedSubmit());
  }

  hasPasswordMismatch(): boolean {
    const password = this.registerForm.controls.password.value;
    const confirmPassword = this.registerForm.controls.confirmPassword.value;
    const wasTouched =
      this.registerForm.controls.confirmPassword.touched || this.hasTriedSubmit();

    return wasTouched && password !== confirmPassword;
  }

  continueWithGoogle(): void {
    if (typeof window !== 'undefined') {
      window.location.assign('https://accounts.google.com/signin');
    }
  }

  submit(): void {
    this.hasTriedSubmit.set(true);
    this.registerForm.markAllAsTouched();

    if (this.registerForm.invalid || this.hasPasswordMismatch()) {
      return;
    }

    this.isSubmitting.set(true);

    setTimeout(() => {
      this.isSubmitting.set(false);
      this.registerForm.reset({
        fullName: '',
        email: '',
        phone: '',
        password: '',
        confirmPassword: '',
        termsAccepted: false,
      });
      this.hasTriedSubmit.set(false);
    }, 750);
  }
}
