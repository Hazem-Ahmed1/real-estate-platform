import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { AdminBuildingDto, AdminBuildingUpsertDto } from '../../../models/AdminBuildingDto';
import { IProject } from '../../../models/IProject';
import { AdminBuildingService } from '../../../services/api/admin-building.service';
import { AdminProjectService } from '../../../services/api/admin-project.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { AlertService } from '../../../shared/services/alert.service';
import { translateAdminBuildingType, translateAdminStatus } from '../admin-list.utils';
import {
  ADMIN_LIST_PAGE_SIZE,
  clampAdminPage,
  extractAdminApiError,
  normalizeBuildingType,
} from '../admin-list.utils';

@Component({
  selector: 'app-admin-buildings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-buildings.html',
  styleUrl: './admin-buildings.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminBuildings implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly fb = inject(FormBuilder);
  private readonly buildingService = inject(AdminBuildingService);
  private readonly adminProjectService = inject(AdminProjectService);
  private readonly snackbar = inject(SnackbarService);
  private readonly alertService = inject(AlertService);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');
  readonly buildings = signal<AdminBuildingDto[]>([]);
  readonly selectedBuilding = signal<AdminBuildingDto | null>(null);
  readonly projects = signal<IProject[]>([]);
  readonly filterProjectId = signal<number | null>(null);
  readonly currentPage = signal(1);
  readonly totalPages = computed(() => {
    const count = this.filteredBuildings().length;
    return Math.max(1, Math.ceil(count / ADMIN_LIST_PAGE_SIZE));
  });

  readonly filteredBuildings = computed(() => {
    const projectId = this.filterProjectId();
    const items = this.buildings();
    if (!projectId) {
      return items;
    }
    return items.filter((b) => b.projectId === projectId);
  });

  readonly pagedBuildings = computed(() => {
    const start = (this.currentPage() - 1) * ADMIN_LIST_PAGE_SIZE;
    return this.filteredBuildings().slice(start, start + ADMIN_LIST_PAGE_SIZE);
  });

  readonly buildingForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    projectId: [null as number | null, [Validators.required]],
    maxArea: [null as number | null, [Validators.required]],
    buildingArea: [null as number | null, [Validators.required]],
    type: ['Sale', [Validators.required]],
    floorCount: [null as number | null, [Validators.required]],
  });

  readonly typeOptions = ['Sale', 'Rent'];
  readonly translateStatusLabel = translateAdminStatus;
  readonly translateBuildingTypeLabel = translateAdminBuildingType;

  canEditType(building: AdminBuildingDto | null): boolean {
    return (building?.units?.length ?? 0) === 0;
  }

  getProjectsForSelectedType(): IProject[] {
    const type = normalizeBuildingType(this.buildingForm.get('type')?.value);
    return this.projects().filter((project) => this.isProjectCompatibleWithType(type, project.status));
  }

  readonly selectedBuildingTypeLabel = computed(() => {
    const type = this.buildingForm.get('type')?.value;
    return type ? this.translateBuildingTypeLabel(type) : '';
  });

  ngOnInit(): void {
    this.loadProjects();
    this.loadBuildings();
    this.buildingForm.get('projectId')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.syncBuildingTypeWithProject());
    this.buildingForm.get('type')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.syncProjectWithType());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProjects(): void {
    this.adminProjectService
      .getProjects({ page: 1, pageSize: 200 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => this.projects.set(response.items ?? []),
      });
  }

  loadBuildings(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.buildingService
      .getBuildings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.buildings.set(data ?? []);
          this.currentPage.set(clampAdminPage(this.currentPage(), this.totalPages()));
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('تعذر تحميل المباني.');
          this.isLoading.set(false);
        },
      });
  }

  onFilterProject(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const value = target.value ? Number(target.value) : null;
    this.filterProjectId.set(Number.isFinite(value) ? value : null);
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.currentPage.set(page);
  }

  selectBuilding(building: AdminBuildingDto): void {
    this.selectedBuilding.set(building);
    this.buildingForm.patchValue({
      name: building.name,
      projectId: building.projectId,
      maxArea: building.maxArea,
      buildingArea: building.buildingArea,
      type: normalizeBuildingType(building.type),
      floorCount: building.floorCount ?? null,
    });
    if (!this.canEditType(building)) {
      this.buildingForm.get('type')?.disable({ emitEvent: false });
    } else {
      this.buildingForm.get('type')?.enable({ emitEvent: false });
    }
    // run duplicate check so inline error appears immediately when editing
    this.checkBuildingName();
  }

  checkBuildingName(): void {
    const control = this.buildingForm.get('name');
    const name = (control?.value ?? '').toString().trim();
    if (!name) return;

    const projectId = Number(this.buildingForm.get('projectId')?.value) || null;
    const existing = this.buildings().find(
      (b) => (b.name ?? '').toString().trim().toLowerCase() === name.toLowerCase() && (projectId == null || b.projectId === projectId)
    );
    // ignore if existing is the one we're editing
    if (existing && (!this.selectedBuilding() || Number(existing.buildingId) !== Number(this.selectedBuilding()!.buildingId))) {
      control?.setErrors({ duplicate: true });
      control?.markAsTouched();
    } else {
      if (control?.hasError('duplicate')) {
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    }
  }

  clearSelection(): void {
    this.selectedBuilding.set(null);
    this.buildingForm.reset({ type: 'Sale', projectId: null });
    this.buildingForm.get('type')?.enable({ emitEvent: false });
  }

  private translateAdminError(message: string): string {
    const text = (message ?? '').toString();
    if (!text) {
      return 'تعذر تنفيذ العملية.';
    }

    const map: Array<[RegExp, string]> = [
      [/same name already exists/i, 'يوجد مبنى بنفس الاسم بالفعل.'],
      [/maximum single-unit area/i, 'أقصى مساحة للوحدة لا يمكن أن تكون أكبر من مساحة المبنى.'],
      [/project status/i, 'لا يمكن إضافة هذا النوع من المباني إلى المشروع الحالي.'],
      [/not found/i, 'العنصر المطلوب غير موجود.'],
      [/failed|error|exception|unable/i, 'تعذر تنفيذ العملية.'],
    ];

    for (const [pattern, arabic] of map) {
      if (pattern.test(text)) {
        return arabic;
      }
    }

    return text;
  }

  submitForm(): void {
    // run duplicate check before evaluating form validity
    this.checkBuildingName();
    if (this.buildingForm.invalid) {
      this.buildingForm.markAllAsTouched();
      this.snackbar.error('يرجى تعبئة جميع الحقول المطلوبة.');
      return;
    }

    const values = this.buildingForm.getRawValue();
    const projectId = Number(values.projectId);
    const maxArea = Number(values.maxArea);
    const buildingArea = Number(values.buildingArea);
    const floorCount = values.floorCount != null ? Number(values.floorCount) : null;

    if (!values.name?.trim() || !Number.isFinite(projectId) || projectId <= 0) {
      this.snackbar.error('اختر مشروعاً صالحاً.');
      return;
    }

    if (!Number.isFinite(maxArea) || !Number.isFinite(buildingArea) || maxArea < 0 || buildingArea < 0) {
      this.snackbar.error('أدخل مساحات صحيحة للمبنى.');
      return;
    }

    const payload: AdminBuildingUpsertDto = {
      name: values.name.trim(),
      projectId,
      maxArea,
      buildingArea,
      type: values.type ?? 'Sale',
      floorCount: Number.isFinite(floorCount) ? floorCount : null,
    };

    const validationError = this.validatePayload(payload);
    if (validationError) {
      this.errorMessage.set(validationError);
      this.snackbar.error(validationError);
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const selected = this.selectedBuilding();
    const request$ = selected
      ? this.buildingService.updateBuilding(selected.buildingId, payload)
      : this.buildingService.createBuilding(payload);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.clearSelection();
        this.snackbar.success('تم حفظ المبنى بنجاح.');
        this.loadBuildings();
      },
      error: (err: unknown) => {
        this.isSaving.set(false);
        const msg = this.translateAdminError(extractAdminApiError(err, 'تعذر حفظ المبنى. تحقق من نوع المبنى ومساحاته وتوافقها مع المشروع.'));
        this.errorMessage.set(msg);
        this.snackbar.error(msg);
      },
    });
  }

  private validatePayload(payload: AdminBuildingUpsertDto): string | null {
    const project = this.projects().find((p) => p.projectId === payload.projectId);
    if (!project) {
      return 'المشروع غير موجود أو غير محمّل. حدّث الصفحة وحاول مرة أخرى.';
    }

    const isSaleProject = project.status === 'Sale' || project.status === 'Sold';
    if (payload.type === 'Sale' && !isSaleProject) {
      return 'لا يمكن إضافة مبنى "للبيع" لمشروع حالته إيجار.';
    }
    if (payload.type === 'Rent' && isSaleProject) {
      return 'لا يمكن إضافة مبنى "للإيجار" لمشروع حالته بيع.';
    }

    // Ensure maximum single-unit area (maxArea) does not exceed the building's total footprint
    if (payload.maxArea > payload.buildingArea) {
      return 'أقصى مساحة للوحدة لا يمكن أن تكون أكبر من مساحة المبنى.';
    }

    return null;
  }

  private syncBuildingTypeWithProject(): void {
    const projectId = Number(this.buildingForm.get('projectId')?.value);
    if (!Number.isFinite(projectId) || projectId <= 0) {
      return;
    }

    const project = this.projects().find((p) => p.projectId === projectId);
    if (!project) {
      return;
    }

    const isSaleProject = project.status === 'Sale' || project.status === 'Sold';
    const expectedType = isSaleProject ? 'Sale' : 'Rent';
    if (this.buildingForm.get('type')?.value !== expectedType) {
      this.buildingForm.patchValue({ type: expectedType }, { emitEvent: false });
    }
  }

  private syncProjectWithType(): void {
    const type = normalizeBuildingType(this.buildingForm.get('type')?.value);
    const projectControl = this.buildingForm.get('projectId');
    const projectId = Number(projectControl?.value);
    if (!Number.isFinite(projectId) || projectId <= 0) {
      return;
    }

    const project = this.projects().find((item) => item.projectId === projectId);
    if (!project) {
      return;
    }

    if (!this.isProjectCompatibleWithType(type, project.status)) {
      projectControl?.patchValue(null, { emitEvent: false });
    }
  }

  private isProjectCompatibleWithType(type: 'Sale' | 'Rent', status: string | null | undefined): boolean {
    const isSaleProject = status === 'Sale' || status === 'Sold';
    return type === 'Sale' ? isSaleProject : !isSaleProject;
  }

  async deleteBuilding(building: AdminBuildingDto): Promise<void> {
    const unitsCount = building.units?.length ?? 0;
    let message = `حذف المبنى "${building.name}"؟`;
    if (unitsCount > 0) {
      message = `المبنى يحتوي على ${unitsCount} وحدة. حذف المبنى سيؤدي إلى إزالة الوحدات والبيانات المرتبطة. هل تريد المتابعة؟`;
    }

    const confirmed = await this.alertService.confirm(message, 'حذف', 'إلغاء');
    if (!confirmed) {
      return;
    }

    this.isSaving.set(true);
    this.buildingService
      .deleteBuilding(building.buildingId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          if (this.selectedBuilding()?.buildingId === building.buildingId) {
            this.clearSelection();
          }
          this.isSaving.set(false);
          this.snackbar.success('تم حذف المبنى.');
          this.loadBuildings();
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          const msg = this.translateAdminError(extractAdminApiError(err, 'تعذر حذف المبنى.'));
          this.errorMessage.set(msg);
          this.snackbar.error(msg);
        },
      });
  }
}
