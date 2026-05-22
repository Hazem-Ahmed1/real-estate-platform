// SweetAlert2 removed; using SnackbarService for notifications
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { InsuranceDto } from '../../../models/lookups.model';
import { LookupService, LookupStatus } from '../../../services/api/lookup.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { AlertService } from '../../../shared/services/alert.service';

import { ADMIN_LIST_PAGE_SIZE, clampAdminPage } from '../admin-list.utils';

@Component({
  selector: 'app-admin-guarantees',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-guarantees.html',
  styleUrl: './admin-guarantees.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminGuarantees implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly fb = inject(FormBuilder);
  private readonly lookupService = inject(LookupService);
  private readonly snackbar = inject(SnackbarService);
  private readonly alertService = inject(AlertService);

  // filter term for list
  readonly filterTerm = signal('');
  // filter status: all, active, inactive
  readonly filterStatus = signal<'all' | 'active' | 'inactive'>('all');
  readonly filteredGuarantees = computed(() => {
    const term = this.filterTerm().trim().toLowerCase();
    const status = this.filterStatus();
    let items = this.guarantees();
    if (status !== 'all') {
      const isActive = status === 'active';
      items = items.filter(g => g.isActive === isActive);
    }
    if (!term) return items;
    return items.filter(g => g.name?.toLowerCase().includes(term));
  });
  readonly currentPage = signal(1);
  readonly totalPages = computed(() => {
    const count = this.filteredGuarantees().length;
    return Math.max(1, Math.ceil(count / ADMIN_LIST_PAGE_SIZE));
  });
  readonly pagedGuarantees = computed(() => {
    const start = (this.currentPage() - 1) * ADMIN_LIST_PAGE_SIZE;
    return this.filteredGuarantees().slice(start, start + ADMIN_LIST_PAGE_SIZE);
  });

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');
  readonly guarantees = signal<InsuranceDto[]>([]);
  readonly selectedGuarantee = signal<InsuranceDto | null>(null);
  readonly guaranteeForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    duration: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
    isActive: [true],
  });

  ngOnInit(): void {
    this.loadGuarantees();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Helper for search input
  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.filterTerm.set(target.value ?? '');
    this.currentPage.set(1);
  }

  // Helper for status filter
  onStatusChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.filterStatus.set(target.value as any);
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.currentPage.set(page);
  }

  loadGuarantees(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.lookupService
      .getInsurances(LookupStatus.All)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.guarantees.set(response ?? []);
          this.currentPage.set(clampAdminPage(this.currentPage(), this.totalPages()));
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          const msg = this.getErrorMessage(error, 'load');
          this.errorMessage.set(msg);
          this.isLoading.set(false);
          this.snackbar.error(msg);
        },
      });
  }

  selectGuarantee(guarantee: InsuranceDto): void {
    this.selectedGuarantee.set(guarantee);
    this.guaranteeForm.patchValue({
      name: guarantee.name,
      duration: guarantee.duration,
      isActive: guarantee.isActive,
    });
  }

  clearSelection(): void {
    this.selectedGuarantee.set(null);
    this.guaranteeForm.reset({ duration: 1, isActive: true });
  }

  submitForm(): void {
    if (this.guaranteeForm.invalid) {
      this.guaranteeForm.markAllAsTouched();
      return;
    }

    const { name, duration, isActive } = this.guaranteeForm.getRawValue();
    if (!name || duration == null) return;

    this.isSaving.set(true);
    this.errorMessage.set('');

    const selected = this.selectedGuarantee();

    // Check for duplicates locally first during Create
    if (!selected) {
      const trimmedName = name.trim().toLowerCase();
      const existing = this.guarantees().find(
        (g) => g.name?.trim().toLowerCase() === trimmedName
      );

      if (existing) {
        this.isSaving.set(false);
        if (existing.isActive) {
          this.snackbar.error('هذا الاسم موجود بالفعل ومفعل.');
        } else {
          // Inactive duplicate - offer manual activation
          this.alertService.confirm('هذا الاسم موجود بالفعل ولكنه غير مفعل. هل تريد تفعيله؟')
            .then((confirm) => {
              if (confirm) {
                this.isSaving.set(true);
                this.lookupService.updateInsurance(existing.insuranceId, {
                  name: existing.name,
                  duration: existing.duration,
                  isActive: true
                })
                  .pipe(takeUntil(this.destroy$))
                  .subscribe({
                    next: () => {
                      this.snackbar.success('تم تفعيل الضمان بنجاح.');
                      this.loadGuarantees();
                      this.clearSelection();
                      this.isSaving.set(false);
                    },
                    error: (e) => {
                      const errMsg = this.getErrorMessage(e, 'save');
                      this.snackbar.error(errMsg);
                      this.isSaving.set(false);
                    }
                  });
              }
            });
        }
        return;
      }
    }

    const request$ = selected
      ? this.lookupService.updateInsurance(selected.insuranceId, { name, duration, isActive: isActive ?? true })
      : this.lookupService.createInsurance({ name, duration });

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (guarantee) => {
        if (selected) {
          this.guarantees.set(
            this.guarantees().map((item) => (item.insuranceId === guarantee.insuranceId ? guarantee : item))
          );
        } else {
          this.guarantees.set([guarantee, ...this.guarantees()]);
        }
        this.isSaving.set(false);
        this.clearSelection();
        this.snackbar.success('تم حفظ الضمان بنجاح.');
      },
      error: async (error: unknown) => {
        const msg = this.getErrorMessage(error, 'save');
        // Check for duplicate inactive error from backend (fallback)
        if (error instanceof HttpErrorResponse && error.status === 400 && error.error && typeof error.error === 'object' && error.error.message?.includes('inactive')) {
          const existingId = error.error.id;
          if (existingId) {
            // Fetch full guarantee to get required fields
            this.lookupService.getInsuranceById(existingId)
              .pipe(takeUntil(this.destroy$))
              .subscribe({
                next: (existingGuarantee) => {
                  this.lookupService.updateInsurance(existingId, {
                    name: existingGuarantee.name,
                    duration: existingGuarantee.duration,
                    isActive: true
                  })
                    .pipe(takeUntil(this.destroy$))
                    .subscribe({
                      next: () => {
                        this.snackbar.success('تم تفعيل العنصر بنجاح.');
                        this.loadGuarantees();
                      },
                      error: (e) => {
                        const errMsg = this.getErrorMessage(e, 'save');
                        this.snackbar.error(errMsg);
                      }
                    });
                },
                error: (e) => {
                  const errMsg = this.getErrorMessage(e, 'load');
                  this.snackbar.error(errMsg);
                }
              });
          }
          this.isSaving.set(false);
          return;
        }
        this.errorMessage.set(msg);
        this.isSaving.set(false);
        this.snackbar.error(msg);
      },
    });
  }

  async deleteGuarantee(guarantee: InsuranceDto): Promise<void> {
    const isLinked = (guarantee.projectCount && guarantee.projectCount > 0) || (guarantee.unitCount && guarantee.unitCount > 0);
    let confirmed = false;

    if (isLinked) {
      confirmed = await this.alertService.confirm(
        `الضمان "${guarantee.name}" مرتبط بـ (${guarantee.projectCount || 0}) مشاريع و (${guarantee.unitCount || 0}) وحدات. هل أنت متأكد من إلغاء تفعيله؟`
      );
    } else {
      confirmed = await this.alertService.confirm(
        `هل أنت متأكد من حذف الضمان "${guarantee.name}" نهائياً؟`
      );
    }

    if (!confirmed) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    if (isLinked) {
      // Linked: Deactivate instead of hard delete
      this.lookupService
        .updateInsurance(guarantee.insuranceId, { name: guarantee.name, duration: guarantee.duration, isActive: false })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.guarantees.set(this.guarantees().map(item => item.insuranceId === guarantee.insuranceId ? { ...item, isActive: false } : item));
            if (this.selectedGuarantee()?.insuranceId === guarantee.insuranceId) {
              this.clearSelection();
            }
            this.isSaving.set(false);
            this.snackbar.success('تم إلغاء تفعيل الضمان.');
          },
          error: (e) => {
            const errMsg = this.getErrorMessage(e, 'save');
            this.errorMessage.set(errMsg);
            this.isSaving.set(false);
            this.snackbar.error(errMsg);
          }
        });
    } else {
      // Not linked: Hard delete
      this.lookupService
        .deleteInsurance(guarantee.insuranceId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.guarantees.set(this.guarantees().filter(g => g.insuranceId !== guarantee.insuranceId));
            if (this.selectedGuarantee()?.insuranceId === guarantee.insuranceId) {
              this.clearSelection();
            }
            this.currentPage.set(clampAdminPage(this.currentPage(), this.totalPages()));
            this.isSaving.set(false);
            this.snackbar.success('تم حذف الضمان بنجاح.');
          },
          error: (error: unknown) => {
            const msg = this.getErrorMessage(error, 'delete');
            this.errorMessage.set(msg);
            this.isSaving.set(false);
            this.snackbar.error(msg);
          }
        });
    }
  }

  private getErrorMessage(error: unknown, action: 'load' | 'save' | 'delete'): string {
    const actionLabel = action === 'load' ? 'تحميل' : action === 'save' ? 'حفظ' : 'حذف';

    if (!(error instanceof HttpErrorResponse)) {
      return `تعذر ${actionLabel} الضمان بسبب خطأ غير متوقع.`;
    }

    if (error.error && typeof error.error === 'string') {
      return error.error;
    }
    
    if (error.error && error.error.message) {
      return error.error.message;
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

    if (action === 'save' && error.status === 400) {
      return 'بيانات الضمان غير صحيحة. تحقق من الحقول المطلوبة.';
    }

    if (action === 'delete' && error.status === 400) {
      return 'لا يمكن حذف الضمان لارتباطه ببيانات أخرى.';
    }

    return `تعذر ${actionLabel} الضمان. حاول مرة أخرى.`;
  }
}
