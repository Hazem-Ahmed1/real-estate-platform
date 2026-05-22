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
// SweetAlert2 removed; using SnackbarService for notifications
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { FeatureDto } from '../../../models/lookups.model';
import { LookupService, LookupStatus } from '../../../services/api/lookup.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { AlertService } from '../../../shared/services/alert.service';

import { ADMIN_LIST_PAGE_SIZE, clampAdminPage } from '../admin-list.utils';

@Component({
  selector: 'app-admin-features',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-features.html',
  styleUrl: './admin-features.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminFeatures implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly fb = inject(FormBuilder);
  private readonly lookupService = inject(LookupService);
  private readonly snackbar = inject(SnackbarService);
  private readonly alertService = inject(AlertService);

  readonly filterTerm = signal('');
  readonly filterStatus = signal<'all' | 'active' | 'inactive'>('all');
  readonly filteredFeatures = computed(() => {
    const term = this.filterTerm().trim().toLowerCase();
    const status = this.filterStatus();
    let items = this.features();
    if (status !== 'all') {
      const isActive = status === 'active';
      items = items.filter(f => f.isActive === isActive);
    }
    if (!term) return items;
    return items.filter(f => f.name?.toLowerCase().includes(term));
  });
  readonly currentPage = signal(1);
  readonly totalPages = computed(() => {
    const count = this.filteredFeatures().length;
    return Math.max(1, Math.ceil(count / ADMIN_LIST_PAGE_SIZE));
  });
  readonly pagedFeatures = computed(() => {
    const start = (this.currentPage() - 1) * ADMIN_LIST_PAGE_SIZE;
    return this.filteredFeatures().slice(start, start + ADMIN_LIST_PAGE_SIZE);
  });
  readonly isSaving = signal(false);
  readonly isLoading = signal(false);
  readonly errorMessage = signal('');
  readonly features = signal<FeatureDto[]>([]);
  readonly selectedFeature = signal<FeatureDto | null>(null);

  readonly featureForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    isActive: [true],
  });

  ngOnInit(): void {
    this.loadFeatures();
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


  loadFeatures(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.lookupService
      .getFeatures(LookupStatus.All)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.features.set(response ?? []);
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

  selectFeature(feature: FeatureDto): void {
    this.selectedFeature.set(feature);
    this.featureForm.patchValue({
      name: feature.name,
      isActive: feature.isActive,
    });
  }

  clearSelection(): void {
    this.selectedFeature.set(null);
    this.featureForm.reset({ isActive: true });
  }

  submitForm(): void {
    if (this.featureForm.invalid) {
      this.featureForm.markAllAsTouched();
      return;
    }

    const { name, isActive } = this.featureForm.getRawValue();
    if (!name) return;

    this.isSaving.set(true);
    this.errorMessage.set('');

    const selected = this.selectedFeature();

    // Check for duplicates locally first during Create
    if (!selected) {
      const trimmedName = name.trim().toLowerCase();
      const existing = this.features().find(
        (f) => f.name?.trim().toLowerCase() === trimmedName
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
                this.lookupService.updateFeature(existing.featureId, { name: existing.name, isActive: true })
                  .pipe(takeUntil(this.destroy$))
                  .subscribe({
                    next: () => {
                      this.snackbar.success('تم تفعيل الميزة بنجاح.');
                      this.loadFeatures();
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
      ? this.lookupService.updateFeature(selected.featureId, { name, isActive: isActive ?? true })
      : this.lookupService.createFeature({ name });

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (feature) => {
        if (selected) {
          this.features.set(
            this.features().map((item) => (item.featureId === feature.featureId ? feature : item))
          );
        } else {
          this.features.set([feature, ...this.features()]);
        }
        this.isSaving.set(false);
        this.clearSelection();
        this.snackbar.success('تم حفظ الميزة بنجاح.');
      },
      error: async (error: unknown) => {
        const msg = this.getErrorMessage(error, 'save');
        // Check for duplicate inactive error from backend (fallback)
        if (error instanceof HttpErrorResponse && error.status === 400 && error.error && typeof error.error === 'object' && error.error.message?.includes('inactive')) {
          const existingId = error.error.id;
          if (existingId) {
            this.lookupService.getFeatureById(existingId)
              .pipe(takeUntil(this.destroy$))
              .subscribe({
                next: (existingFeature) => {
                  this.lookupService.updateFeature(existingId, { name: existingFeature.name, isActive: true })
                    .pipe(takeUntil(this.destroy$))
                    .subscribe({
                      next: () => {
                        this.snackbar.success('تم تفعيل العنصر بنجاح.');
                        this.loadFeatures();
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

  async deleteFeature(feature: FeatureDto): Promise<void> {
    const isLinked = (feature.projectCount && feature.projectCount > 0) || (feature.unitCount && feature.unitCount > 0);
    let confirmed = false;

    if (isLinked) {
      confirmed = await this.alertService.confirm(
        `الميزة "${feature.name}" مرتبطة بـ (${feature.projectCount || 0}) مشاريع و (${feature.unitCount || 0}) وحدات. هل أنت متأكد من إلغاء تفعيلها؟`
      );
    } else {
      confirmed = await this.alertService.confirm(
        `هل أنت متأكد من حذف الميزة "${feature.name}" نهائياً؟`
      );
    }

    if (!confirmed) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    if (isLinked) {
      // Linked: Deactivate feature instead of deleting
      this.lookupService.updateFeature(feature.featureId, { name: feature.name, isActive: false })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.features.set(this.features().map(item => item.featureId === feature.featureId ? { ...item, isActive: false } : item));
            if (this.selectedFeature()?.featureId === feature.featureId) {
              this.clearSelection();
            }
            this.isSaving.set(false);
            this.snackbar.success('تم إلغاء تفعيل الميزة.');
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
      this.lookupService.deleteFeature(feature.featureId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.features.set(this.features().filter(f => f.featureId !== feature.featureId));
            if (this.selectedFeature()?.featureId === feature.featureId) {
              this.clearSelection();
            }
            this.currentPage.set(clampAdminPage(this.currentPage(), this.totalPages()));
            this.isSaving.set(false);
            this.snackbar.success('تم حذف الميزة بنجاح.');
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
      return `تعذر ${actionLabel} الميزة بسبب خطأ غير متوقع.`;
    }

    // Handle specific backend validation/constraint errors
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
      return 'بيانات الميزة غير صحيحة. تحقق من الحقول المطلوبة.';
    }

    if (action === 'delete' && error.status === 400) {
      return 'لا يمكن حذف الميزة لارتباطها ببيانات أخرى.';
    }

    return `تعذر ${actionLabel} الميزة. حاول مرة أخرى.`;
  }
}
