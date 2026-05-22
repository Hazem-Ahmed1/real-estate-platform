import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { IProject, IProjectDetails } from '../../../models/IProject';
import { FeatureDto, InsuranceDto } from '../../../models/IProject';
import { AdminProjectService } from '../../../services/api/admin-project.service';
import { ProjectMap } from '../../../shared/components/project-map/project-map';
import { LookupService, LookupStatus } from '../../../services/api/lookup.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { AlertService } from '../../../shared/services/alert.service';

import { ADMIN_LIST_PAGE_SIZE, normalizePagedResponse, translateAdminStatus } from '../admin-list.utils';

@Component({
  selector: 'app-admin-projects',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ProjectMap],
  templateUrl: './admin-projects.html',
  styleUrl: './admin-projects.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminProjects implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly fb = inject(FormBuilder);
  private readonly adminProjectService = inject(AdminProjectService);
  private readonly lookupService = inject(LookupService);
  private readonly snackbar = inject(SnackbarService);
  private readonly alertService = inject(AlertService);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');
  readonly projects = signal<IProject[]>([]);
  readonly selectedProject = signal<IProjectDetails | null>(null);
  readonly currentPage = signal(1);
  readonly totalPages = signal(1);

  readonly features = signal<FeatureDto[]>([]);
  readonly insurances = signal<InsuranceDto[]>([]);
  readonly selectedFeatureIds = signal<Set<number>>(new Set());
  readonly selectedInsuranceIds = signal<Set<number>>(new Set());

  readonly thumbnailName = signal('لم يتم اختيار ملف');
  readonly imagesSummary = signal('لم يتم اختيار ملفات');
  readonly panoramaName = signal('لم يتم اختيار ملف');
  readonly videoName = signal('لم يتم اختيار ملف');

  readonly statusOptions = ['Sale', 'Rent', 'Sold', 'Rented'];
  readonly createStatusOptions = ['Sale', 'Rent'];
  readonly translateStatusLabel = translateAdminStatus;

  readonly statusOptionsForForm = computed(() =>
    this.selectedProject() && ((this.selectedProject()?.buildingsNumber ?? 0) > 0 || (this.selectedProject()?.unitsNumber ?? 0) > 0)
      ? this.statusOptions
      : this.createStatusOptions
  );

  readonly projectForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    region: [''],
    address: [''],
    latitude: [null as number | null, [Validators.required]],
    longitude: [null as number | null, [Validators.required]],
    landArea: [null as number | null, [Validators.required]],
    buildUpArea: [null as number | null, [Validators.required]],
    status: ['Sale', [Validators.required]],
    thumbnail: [null as File | null],
    images: [null as FileList | null],
    panorama: [null as File | null],
    video: [null as File | null],
  });

  readonly showMap = signal(false);
  readonly mapLat = signal(24.7136);
  readonly mapLng = signal(46.6753);
  readonly mapQuery = signal('');
  readonly mapCircleMeters = signal<number | null>(null);
  /** Full geocoded address saved on create/update; visible address field shows city only. */
  readonly storedFullAddress = signal('');

  openMap(): void {
    const cityVal = (this.projectForm.get('city')?.value ?? '').toString().trim();
    if (!cityVal) {
      this.snackbar.error('اختر المدينة أولاً قبل فتح الخريطة.');
      return;
    }

    const lat = this.projectForm.get('latitude')?.value ?? this.mapLat();
    const lng = this.projectForm.get('longitude')?.value ?? this.mapLng();
    this.mapLat.set(lat ?? 24.7136);
    this.mapLng.set(lng ?? 46.6753);
    this.mapQuery.set(cityVal);
    this.mapCircleMeters.set(this.computeSelectionRadiusMeters(this.projectForm.get('buildUpArea')?.value ?? this.projectForm.get('landArea')?.value));
    this.showMap.set(true);
  }

  closeMap(): void {
    this.showMap.set(false);
  }

  onMapSelection(ev: { lat: number; lng: number; addressLine1: string; addressLine2: string; places: any[] }): void {
    const rawFull = `${ev.addressLine1} ${ev.addressLine2}`.trim();
    const selectedCity = (this.projectForm.get('city')?.value ?? '').toString().trim();

    // Validate selection is inside the chosen city (best-effort by text match against returned address)
    if (selectedCity) {
      const lower = rawFull.toLowerCase();
      const cityLower = selectedCity.toLowerCase();
      if (!lower.includes(cityLower)) {
        this.snackbar.error('الموقع المحدد ليس ضمن المدينة المختارة. اختر موقعاً داخل المدينة أو غيّر المدينة أولاً.');
        return;
      }
    }

    const city = this.extractCityFromAddress(ev.addressLine1) ?? selectedCity ?? '';
    const region = this.extractRegionFromAddress(ev.addressLine1) ?? '';
    const normalized = this.ensureAddressContainsCity(rawFull, city);

    this.storedFullAddress.set(normalized);
    this.projectForm.patchValue({
      latitude: ev.lat,
      longitude: ev.lng,
      address: city,
      city,
      region,
    });
  }

  ngOnInit(): void {
    this.loadLookups();
    this.loadProjects(1);
    this.projectForm.get('city')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((city) => {
        const trimmed = (city ?? '').trim();
        if (trimmed) {
          this.projectForm.patchValue({ address: trimmed }, { emitEvent: false });
        }
      });
  }

  checkProjectName(): void {
    const control = this.projectForm.get('name');
    const name = (control?.value ?? '').toString().trim();
    if (!name) return;

    const existing = this.projects().find((p) => (p.name ?? '').toString().trim().toLowerCase() === name.toLowerCase());
    // if the existing item is the one currently selected for edit, ignore as it's not a duplicate
    if (existing && (!this.selectedProject() || existing.projectId !== this.selectedProject()!.projectId)) {
      control?.setErrors({ duplicate: true });
      control?.markAsTouched();
    } else {
      // clear duplicate error if previously set
      if (control?.hasError('duplicate')) {
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProjects(page: number): void {
    this.currentPage.set(page);
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.adminProjectService
      .getProjects({ page, pageSize: ADMIN_LIST_PAGE_SIZE })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const pageData = normalizePagedResponse(response);
          this.projects.set(pageData.items);
          this.totalPages.set(pageData.totalPages);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('تعذر تحميل المشاريع. حاول مرة أخرى.');
          this.isLoading.set(false);
        },
      });
  }

  loadLookups(): void {
    this.lookupService
      .getFeatures(LookupStatus.All)
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => this.features.set((data ?? []).filter(f => f.isActive)));

    this.lookupService
      .getInsurances(LookupStatus.All)
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => this.insurances.set((data ?? []).filter(i => i.isActive)));
  }

  selectProject(project: IProject): void {
    this.isLoading.set(true);
    this.adminProjectService
      .getProject(project.projectId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (details) => {
          this.selectedProject.set(details);
          const city = details.cityName ?? '';
          const fullAddress = details.address ?? city;
          this.storedFullAddress.set(this.ensureAddressContainsCity(fullAddress, city));
          this.projectForm.patchValue({
            name: details.name,
            city,
            region: details.regionName ?? '',
            address: city,
            latitude: details.latitude ?? null,
            longitude: details.longitude ?? null,
            landArea: project.landArea ?? null,
            buildUpArea: details.buildUpArea ?? null,
            status:
              ((details.buildingsNumber ?? 0) > 0 || (details.unitsNumber ?? 0) > 0)
                ? (details.status ?? 'Sale')
                : (this.createStatusOptions.includes(details.status ?? '') ? details.status : 'Sale'),
          });
          this.selectedFeatureIds.set(new Set(details.features.map((f) => f.featureId)));
          this.selectedInsuranceIds.set(new Set(details.insurance.map((i) => i.insuranceId)));
          this.resetMediaLabels();
          // run duplicate check so inline error appears immediately when editing
          this.checkProjectName();
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('تعذر تحميل تفاصيل المشروع.');
          this.isLoading.set(false);
        },
      });
  }

  clearSelection(): void {
    this.selectedProject.set(null);
    this.storedFullAddress.set('');
    this.projectForm.reset({ status: 'Sale' });
    this.selectedFeatureIds.set(new Set());
    this.selectedInsuranceIds.set(new Set());
    this.resetMediaLabels();
  }

  private translateAdminError(message: string): string {
    const text = (message ?? '').toString();
    if (!text) {
      return 'تعذر تنفيذ العملية.';
    }

    const map: Array<[RegExp, string]> = [
      [/same name already exists/i, 'يوجد مشروع بنفس الاسم بالفعل.'],
      [/cannot change the project status/i, 'لا يمكن تغيير حالة المشروع بعد إضافة مبانٍ.'],
      [/build-up area must be less than or equal to land area/i, 'مساحة البناء يجب أن تكون أقل من أو تساوي مساحة الأرض.'],
      [/total building area must be less than or equal to build-up area/i, 'إجمالي مساحة المباني يجب أن يكون أقل من أو يساوي مساحة البناء.'],
      [/invalid/i, 'البيانات غير صحيحة.'],
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

  toggleFeature(featureId: number, event: Event): void {
    const target = event.target as HTMLInputElement;
    const current = new Set(this.selectedFeatureIds());
    if (target.checked) {
      current.add(featureId);
    } else {
      current.delete(featureId);
    }
    this.selectedFeatureIds.set(current);
  }

  toggleInsurance(insuranceId: number, event: Event): void {
    const target = event.target as HTMLInputElement;
    const current = new Set(this.selectedInsuranceIds());
    if (target.checked) {
      current.add(insuranceId);
    } else {
      current.delete(insuranceId);
    }
    this.selectedInsuranceIds.set(current);
  }

  onThumbnailChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0] ?? null;

    if (file && !this.isValidImageFile(file)) {
      this.snackbar.error('يرجى اختيار صورة صالحة (JPG, PNG, WebP)');
      return;
    }

    this.projectForm.patchValue({ thumbnail: file });
    this.thumbnailName.set(file?.name ?? 'لم يتم اختيار ملف');
  }

  onImagesChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const files = target.files ?? null;

    if (files) {
      const invalidFiles = Array.from(files).filter(f => !this.isValidImageFile(f));
      if (invalidFiles.length > 0) {
        this.snackbar.error('جميع الصور يجب أن تكون (JPG, PNG, WebP)');
        return;
      }
    }

    this.projectForm.patchValue({ images: files });
    if (!files || files.length === 0) {
      this.imagesSummary.set('لم يتم اختيار ملفات');
      return;
    }
    this.imagesSummary.set(`تم اختيار ${files.length} صورة`);
  }

  onPanoramaChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0] ?? null;

    if (file && !this.isValidImageFile(file)) {
      this.snackbar.error('يرجى اختيار صورة صالحة (JPG, PNG, WebP)');
      return;
    }

    this.projectForm.patchValue({ panorama: file });
    this.panoramaName.set(file?.name ?? 'لم يتم اختيار ملف');
  }

  onVideoChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0] ?? null;

    if (file && !this.isValidVideoFile(file)) {
      this.snackbar.error('يرجى اختيار فيديو صالح (MP4, WebM, OGG)');
      return;
    }

    this.projectForm.patchValue({ video: file });
    this.videoName.set(file?.name ?? 'لم يتم اختيار ملف');
  }

  submitForm(): void {
    // ensure duplicate check runs before validity is evaluated
    this.checkProjectName();
    if (this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      this.snackbar.error('يرجى تعبئة جميع الحقول المطلوبة بما فيها الإحداثيات ومساحات الأرض والبناء.');
      return;
    }

    const values = this.projectForm.getRawValue();
    if (values.latitude == null || values.longitude == null) {
      this.snackbar.error('يرجى تحديد الموقع من الخريطة أو إدخال خط العرض وخط الطول.');
      return;
    }

    if (values.landArea != null && values.buildUpArea != null && values.buildUpArea > values.landArea) {
      this.snackbar.error('مساحة البناء يجب أن تكون أقل من أو تساوي مساحة الأرض.');
      return;
    }

    const payload = this.buildPayload();
    if (!payload) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const selected = this.selectedProject();
    const request$ = selected
      ? this.adminProjectService.updateProject(selected.projectId, payload)
      : this.adminProjectService.createProject(payload);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.clearSelection();
        this.loadProjects(this.currentPage());
        this.snackbar.success('تم حفظ المشروع بنجاح.');
      },
      error: (err: any) => {
        this.isSaving.set(false);
        const msg = this.translateAdminError(err?.error?.message || err?.message || 'تعذر حفظ المشروع.');
        this.errorMessage.set(msg);
        this.snackbar.error(msg);
      },
    });
  }

  async deleteProject(project: IProject): Promise<void> {
    // Load full details to check for buildings/units before deleting
    this.isSaving.set(true);
    this.adminProjectService.getProject(project.projectId).pipe(takeUntil(this.destroy$)).subscribe({
      next: async (details) => {
        const buildings = details.buildingsNumber ?? 0;
        const units = details.unitsNumber ?? 0;
        let message = `حذف المشروع "${project.name}"؟`;
        if (buildings > 0 || units > 0) {
          message = `المشروع يحتوي على ${buildings} مبنى و ${units} وحدة. حذف المشروع سيؤدي إلى إزالة جميع البيانات المرتبطة (وحدات، وسائط...). هل تريد المتابعة؟`;
        }

        const confirmed = await this.alertService.confirm(message, 'حذف', 'إلغاء');
        if (!confirmed) {
          this.isSaving.set(false);
          return;
        }

        this.adminProjectService.deleteProject(project.projectId).pipe(takeUntil(this.destroy$)).subscribe({
          next: () => {
            this.projects.set(this.projects().filter((item) => item.projectId !== project.projectId));
            if (this.selectedProject()?.projectId === project.projectId) {
              this.clearSelection();
            }
            this.isSaving.set(false);
            this.snackbar.success('تم حذف المشروع.');
          },
          error: (err: any) => {
            this.isSaving.set(false);
            const msg = this.translateAdminError(err?.error?.message || err?.message || 'تعذر حذف المشروع.');
            this.errorMessage.set(msg);
            this.snackbar.error(msg);
          },
        });
      },
      error: () => {
        this.isSaving.set(false);
          this.snackbar.error('تعذر جلب تفاصيل المشروع. حاول مرة أخرى.');
      },
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.loadProjects(page);
  }

  private buildPayload(): FormData | null {
    const values = this.projectForm.getRawValue();
    if (!values.name || !values.city) {
      return null;
    }

    const city = values.city.trim();
    const fullAddress = this.ensureAddressContainsCity(this.storedFullAddress().trim() || city, city);

    const payload = new FormData();
    payload.append('name', values.name);
    payload.append('city', city);
    payload.append('address', fullAddress);
    payload.append('latitude', String(values.latitude));
    payload.append('longitude', String(values.longitude));
    payload.append('landArea', String(values.landArea ?? 0));
    payload.append('buildUpArea', String(values.buildUpArea ?? 0));

    if (values.region?.trim()) {
      payload.append('region', values.region.trim());
    }

    const status = this.selectedProject() ? values.status : (values.status === 'Rent' ? 'Rent' : 'Sale');
    payload.append('status', status ?? 'Sale');

    for (const id of this.selectedFeatureIds()) {
      payload.append('featureIds', String(id));
    }

    for (const id of this.selectedInsuranceIds()) {
      payload.append('insuranceIds', String(id));
    }

    if (values.thumbnail) payload.append('thumbnailImage', values.thumbnail);
    if (values.images) {
      Array.from(values.images).forEach((file) => payload.append('images', file));
    }
    if (values.panorama) payload.append('panorama360', values.panorama);
    if (values.video) payload.append('videoFile', values.video);

    return payload;
  }

  private resetMediaLabels(): void {
    this.thumbnailName.set('لم يتم اختيار ملف');
    this.imagesSummary.set('لم يتم اختيار ملفات');
    this.panoramaName.set('لم يتم اختيار ملف');
    this.videoName.set('لم يتم اختيار ملف');
  }

  private computeSelectionRadiusMeters(area?: number | null): number | null {
    if (area == null || area <= 0) {
      return null;
    }

    return Math.max(150, Math.min(2200, Math.sqrt(area / Math.PI)));
  }

  private extractCityFromAddress(addressLine1: string): string | null {
    const parts = (addressLine1 || '').split('،').map((part) => part.trim()).filter(Boolean);
    return parts.at(-1) ?? null;
  }

  private extractRegionFromAddress(addressLine1: string): string | null {
    const parts = (addressLine1 || '').split('،').map((part) => part.trim()).filter(Boolean);
    if (parts.length >= 2) {
      return parts[parts.length - 2] ?? parts[0];
    }
    return parts[0] ?? null;
  }

  private ensureAddressContainsCity(address: string, city: string): string {
    const trimmedCity = city.trim();
    const trimmedAddress = address.trim();
    if (!trimmedCity) {
      return trimmedAddress;
    }
    if (!trimmedAddress) {
      return trimmedCity;
    }
    // Normalize repeated city occurrence: split by Arabic comma and remove duplicate trailing city parts
    const parts = trimmedAddress.split('،').map(p => p.trim()).filter(Boolean);
    // Remove trailing parts that equal city (duplicates)
    while (parts.length > 1 && parts.at(-1)?.toLowerCase() === trimmedCity.toLowerCase()) {
      parts.pop();
    }
    const base = parts.join(' ، ');
    if (base.toLowerCase().includes(trimmedCity.toLowerCase())) {
      return base;
    }
    return `${base} ، ${trimmedCity}`;
  }

  private isValidImageFile(file: File): boolean {
    const validTypes = ['image/jpeg', 'image/png', 'image/webp'];
    return validTypes.includes(file.type);
  }

  private isValidVideoFile(file: File): boolean {
    const validTypes = ['video/mp4', 'video/webm', 'video/ogg'];
    return validTypes.includes(file.type);
  }
}
