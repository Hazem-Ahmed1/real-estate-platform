import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { UnitCardModel } from '../../../models/IUnit';
import { FeatureDto, InsuranceDto, IProject } from '../../../models/IProject';
import { AdminBuildingDto } from '../../../models/AdminBuildingDto';
import { AdminUnitService } from '../../../services/api/admin-unit.service';
import { AdminBuildingService } from '../../../services/api/admin-building.service';
import { AdminProjectService } from '../../../services/api/admin-project.service';
const OVERPASS_ENDPOINTS = [
  'https://overpass-api.de/api/interpreter',
  'https://overpass.kumi.systems/api/interpreter',
  'https://overpass.openstreetmap.fr/api/interpreter',
];
import { ProjectMap } from '../../../shared/components/project-map/project-map';
import { LookupService, LookupStatus } from '../../../services/api/lookup.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { AlertService } from '../../../shared/services/alert.service';
import { ADMIN_LIST_PAGE_SIZE, normalizeBuildingType, normalizePagedResponse, translateAdminBuildingType, translateAdminStatus } from '../admin-list.utils';

@Component({
  selector: 'app-admin-units',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ProjectMap],
  templateUrl: './admin-units.html',
  styleUrl: './admin-units.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUnits implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly fb = inject(FormBuilder);
  private readonly unitService = inject(AdminUnitService);
  private readonly buildingService = inject(AdminBuildingService);
  private readonly adminProjectService = inject(AdminProjectService);
  private readonly route = inject(ActivatedRoute);
  private readonly lookupService = inject(LookupService);
  private readonly snackbar = inject(SnackbarService);
  private readonly alertService = inject(AlertService);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');
  readonly units = signal<UnitCardModel[]>([]);
  readonly selectedUnit = signal<UnitCardModel | null>(null);
  readonly projects = signal<IProject[]>([]);
  readonly buildings = signal<AdminBuildingDto[]>([]);
  readonly features = signal<FeatureDto[]>([]);
  readonly insurances = signal<InsuranceDto[]>([]);

  readonly selectedFeatureIds = signal<Set<number>>(new Set());
  readonly selectedInsuranceIds = signal<Set<number>>(new Set());
  readonly nearbyFacilities = signal<any[]>([]);
  readonly selectedRegion = signal<string | null>(null);
  readonly selectedCity = signal('');

  readonly currentPage = signal(1);
  readonly totalPages = signal(1);
  readonly filterProjectId = signal<number | null>(null);
  readonly filterBuildingId = signal<number | null>(null);
  readonly selectedStatus = signal<string | null>(null);

  readonly filteredBuildings = computed(() => {
    const projectId = this.filterProjectId();
    if (!projectId) {
      return this.buildings();
    }
    return this.buildings().filter((b: AdminBuildingDto) => b.projectId === projectId);
  });

  readonly thumbnailName = signal('لم يتم اختيار ملف');
  readonly imagesSummary = signal('لم يتم اختيار ملفات');
  readonly designsSummary = signal('لم يتم اختيار ملفات');
  readonly panoramaName = signal('لم يتم اختيار ملف');
  readonly videoName = signal('لم يتم اختيار ملف');

  readonly statusOptions = ['Sale', 'Rent', 'Sold', 'Rented'];
  readonly translateStatusLabel = translateAdminStatus;
  readonly translateBuildingTypeLabel = translateAdminBuildingType;
  readonly typeOptions = ['Apartment', 'Villa', 'Duplex', 'Office'];
  readonly facilityTypes = [
    'Mosque', 'School', 'Hospital', 'Restaurant', 'Park',
    'Bank', 'Pharmacy', 'SuperMarket', 'Club', 'Other'
  ];

  readonly unitForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    status: [null as string | null, [Validators.required]],
    buildingId: [null as number | null, [Validators.required]],
    rooms: [1, [Validators.required, Validators.min(1)]],
    salons: [1, [Validators.required, Validators.min(1)]],
    area: [null as number | null, [Validators.required, Validators.min(10)]],
    bathrooms: [1, [Validators.required, Validators.min(1)]],
    floor: [1, [Validators.required, Validators.min(1)]],
    price: [null as number | null, [Validators.required, Validators.min(50)]],
    type: ['Apartment', [Validators.required]],
    streetCount: [1, [Validators.required, Validators.min(1), Validators.max(4)]],
    // city/region are derived from the project and not editable here
    street: [''],
    // address is provided by the map picker; keep it optional so manual lat/lng can be used
    address: [''],
    latitude: [null as number | null, [Validators.required]],
    longitude: [null as number | null, [Validators.required]],
    thumbnail: [null as File | null],
    images: [null as FileList | null],
    designs: [null as FileList | null],
    panorama: [null as File | null],
    video: [null as File | null],
  });

  readonly showMap = signal(false);
  readonly mapLat = signal(24.7136);
  readonly mapLng = signal(46.6753);
  readonly mapAnchorLat = signal<number | null>(null);
  readonly mapAnchorLng = signal<number | null>(null);
  readonly mapQuery = signal('');
  readonly mapCircleMeters = signal<number | null>(null);

  readonly filteredBuildingsByStatus = computed(() => {
    const status = this.selectedStatus();
    if (!status) {
      return [] as AdminBuildingDto[];
    }

    return this.buildings().filter((building) => this.isBuildingCompatibleWithStatus(building, status));
  });

  ngOnInit(): void {
    this.loadLookups();
    this.loadProjects();
    this.loadBuildings();
    this.unitForm.get('status')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((status) => {
        this.selectedStatus.set(status ?? null);
        this.syncBuildingWithStatus(status ?? null);
      });
    this.unitForm.get('buildingId')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((value: any) => {
        this.syncLocationFromBuilding(Number(value) || null);
        // re-check area limit when building changes
        this.checkAreaLimit();
      });

    this.unitForm.get('area')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.checkAreaLimit());
    // read query params to pre-filter
    const p = Number(this.route.snapshot.queryParamMap.get('projectId')) || null;
    const b = Number(this.route.snapshot.queryParamMap.get('buildingId')) || null;
    if (p) this.filterProjectId.set(p);
    if (b) this.filterBuildingId.set(b);
    this.loadUnits(1);
  }

  private checkAreaLimit(): void {
    const control = this.unitForm.get('area');
    const areaVal = Number(control?.value ?? 0);
    if (!control) return;

    const buildingId = Number(this.unitForm.get('buildingId')?.value) || null;
    if (!buildingId || !Number.isFinite(areaVal) || areaVal <= 0) {
      const currentErrors = { ...(control.errors ?? {}) } as Record<string, any>;
      if (currentErrors['maxExceeded']) {
        delete currentErrors['maxExceeded'];
        control.setErrors(Object.keys(currentErrors).length > 0 ? currentErrors : null);
      }
      return;
    }

    const building = this.buildings().find((b: AdminBuildingDto) => Number(b.buildingId) === Number(buildingId));
    const maxArea = building?.maxArea ?? null;
    if (maxArea != null && areaVal > Number(maxArea)) {
      control.setErrors({ ...(control.errors ?? {}), maxExceeded: true });
      control.markAsTouched();
    } else {
      const currentErrors = { ...(control.errors ?? {}) } as Record<string, any>;
      if (currentErrors['maxExceeded']) {
        delete currentErrors['maxExceeded'];
        control.setErrors(Object.keys(currentErrors).length > 0 ? currentErrors : null);
      }
    }
  }

  checkUnitName(): void {
    const control = this.unitForm.get('name');
    const name = (control?.value ?? '').toString().trim();
    if (!name) return;
    const buildingId = Number(this.unitForm.get('buildingId')?.value) || null;
    const buildingName = buildingId ? this.buildings().find((b: AdminBuildingDto) => b.buildingId === buildingId)?.name : null;
    const projectName = buildingId ? this.projects().find((p: IProject) => p.projectId === this.buildings().find((b: AdminBuildingDto) => b.buildingId === buildingId)?.projectId)?.name : null;

    const existing = this.units().find((u: UnitCardModel) => {
      const sameName = (u.name ?? '').toString().trim().toLowerCase() === name.toLowerCase();
      if (!sameName) return false;
      if (buildingName) return (u.buildingName ?? '') === buildingName;
      if (projectName) return (u.projectName ?? '') === projectName;
      return true;
    });
    // ignore if existing is the same unit we're editing
    if (existing && (!this.selectedUnit() || existing.unitId !== this.selectedUnit()!.unitId)) {
      control?.setErrors({ duplicate: true });
      control?.markAsTouched();
    } else {
      if (control?.hasError('duplicate')) {
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    }
  }

  openMap(): void {
    const buildingId = Number(this.unitForm.get('buildingId')?.value) || null;
    if (!buildingId) {
      this.errorMessage.set('اختر المبنى أولاً قبل فتح الخريطة.');
      return;
    }

    this.syncLocationFromBuilding(buildingId);
    const building = this.buildings().find((item: AdminBuildingDto) => item.buildingId === buildingId);
    const project = building
      ? this.projects().find((item: IProject) => item.projectId === building.projectId)
      : undefined;

    const unitLat = this.unitForm.get('latitude')?.value;
    const unitLng = this.unitForm.get('longitude')?.value;

    const applyMapState = (projectLat: number | null, projectLng: number | null): void => {
      const anchorLat = projectLat ?? unitLat ?? 24.7136;
      const anchorLng = projectLng ?? unitLng ?? 46.6753;
      this.mapAnchorLat.set(anchorLat);
      this.mapAnchorLng.set(anchorLng);
      this.mapLat.set(anchorLat);
      this.mapLng.set(anchorLng);
      // set map query to the project's city
      this.mapQuery.set((project?.city ?? '').trim());
      this.showMap.set(true);
    };

    if (!project) {
      applyMapState(null, null);
      return;
    }

    this.adminProjectService
      .getProject(project.projectId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (details: any) => {
          applyMapState(details.latitude ?? null, details.longitude ?? null);
        },
        error: () => applyMapState(null, null),
      });
  }

  closeMap(): void {
    this.showMap.set(false);
  }

  onMapSelection(ev: { lat: number; lng: number; addressLine1: string; addressLine2: string; places: any[] }): void {
    // ensure selection is within selected project's city
    const buildingId = Number(this.unitForm.get('buildingId')?.value) || null;
    const building = buildingId ? this.buildings().find((b: AdminBuildingDto) => b.buildingId === buildingId) : undefined;
    const project = building ? this.projects().find((p: IProject) => p.projectId === building.projectId) : undefined;
    if (!project) {
      this.snackbar.error('اختر المبنى/المشروع أولاً قبل اختيار الموقع.');
      return;
    }

    const cityName = (project.city ?? '').toString().toLowerCase();
    const addrCombined = `${ev.addressLine1} ${ev.addressLine2}`.toLowerCase();
    if (cityName && !addrCombined.includes(cityName)) {
      this.snackbar.error('يجب أن تكون النقطة داخل نفس مدينة المشروع. اختر نقطة داخل المدينة.');
      return;
    }

    // Selection will be validated by the map component; here assume emitted selection is valid.

    // build address and remove city name from it to avoid repeated city in saved address
    const rawAddress = `${ev.addressLine1} ${ev.addressLine2}`.trim();
    const cleanedAddress = cityName
      ? rawAddress.replace(new RegExp(cityName, 'ig'), '').replace(/\s{2,}/g, ' ').replace(/^[,\s]+|[,\s]+$/g, '')
      : rawAddress;

    this.unitForm.patchValue({
      latitude: ev.lat,
      longitude: ev.lng,
      address: cleanedAddress,
    });
    // ensure city/region reflect the selected project's values for payload and display
    this.selectedCity.set(project.city ?? '');
    // derive region heuristically and store only address/coords in the form; region will be taken from the project on submit
    const parts = (ev.addressLine1 || '').split('،').map(p => p.trim()).filter(Boolean);
    if (parts.length >= 1) {
      // Prefer map-derived region (first segment) when available
      this.selectedRegion.set(parts[0]);
    } else {
      this.selectedRegion.set(null);
    }

    // map nearby places into facilities array with approximate distances
    const facilities = (ev.places || []).map((p: any) => ({
      name: p.name,
      type: this.mapPoiToFacilityType(p),
      distance: this.formatDistanceMeters(this.computeDistanceMeters(ev.lat, ev.lng, p.lat, p.lng)),
      area: 0,
      latitude: p.lat,
      longitude: p.lng,
      tags: p.tags ?? undefined,
      selected: true,
    }));

    this.nearbyFacilities.set(this.dedupeNearbyFacilities(facilities));
  }

  handleMapOutOfBounds(): void {
    this.snackbar.error('النقطة المختارة خارج نطاق المشروع (خارج الدايرة). اختر نقطة داخل النطاق.');
  }

  mapPoiToFacilityType(p: any): string {
    // Prefer tags if available
    const tags = p?.tags ?? {};

    const name = (p?.name ?? '').toString().toLowerCase();

    const tagChecks: Array<[() => boolean, string]> = [
      [() => !!(tags.amenity && /mosque|place_of_worship|worship/.test(tags.amenity)) || /mosque|مسجد/.test(name), 'Mosque'],
      [() => !!(tags.amenity && /school|university/.test(tags.amenity)) || /school|مدرسة|جامعة/.test(name), 'School'],
      [() => !!(tags.healthcare || tags.amenity && /hospital|clinic|healthcare/.test(tags.amenity)) || /hospital|مستشفى|عيادة/.test(name), 'Hospital'],
      [() => !!(tags.amenity && /restaurant|cafe/.test(tags.amenity)) || /restaurant|مطعم|كافيه/.test(name), 'Restaurant'],
      [() => !!(tags.leisure && /park|garden/.test(tags.leisure)) || /park|حديقة|منتزه/.test(name), 'Park'],
      [() => !!(tags.shop && /supermarket|grocery|convenience|mall/.test(tags.shop)) || /supermarket|سوبر|بقال/.test(name), 'SuperMarket'],
      [() => !!(tags.amenity && /bank/.test(tags.amenity)) || /bank|بنك|atm|صراف/.test(name), 'Bank'],
      [() => !!(tags.amenity && /pharmacy/.test(tags.amenity)) || /pharmacy|صيدلية/.test(name), 'Pharmacy'],
      [() => !!(tags.leisure && /sports|club/.test(tags.leisure)) || /club|نادي|رياضة/.test(name), 'Club'],
    ];

    for (const [check, type] of tagChecks) {
      try {
        if (check()) return type;
      } catch {
        // ignore
      }
    }

    return 'Other';
  }

  facilityIconClass(type: string): string {
    switch ((type ?? '').toLowerCase()) {
      case 'mosque': return 'fa-mosque';
      case 'school': return 'fa-graduation-cap';
      case 'hospital': return 'fa-hospital';
      case 'restaurant': return 'fa-utensils';
      case 'park': return 'fa-tree';
      case 'bank': return 'fa-building-columns';
      case 'pharmacy': return 'fa-prescription-bottle-medical';
      case 'supermarket': return 'fa-cart-shopping';
      case 'club': return 'fa-dumbbell';
      default: return 'fa-location-dot';
    }
  }

  selectAllFacilities(): void {
    const items = this.nearbyFacilities().map((f: any) => ({ ...f, selected: true }));
    this.nearbyFacilities.set(items);
  }

  deselectAllFacilities(): void {
    const items = this.nearbyFacilities().map((f: any) => ({ ...f, selected: false }));
    this.nearbyFacilities.set(items);
  }

  private computeDistanceMeters(lat1: number, lng1: number, lat2: number, lng2: number): number {
    const R = 6371000; // meters
    const toRad = (v: number) => (v * Math.PI) / 180;
    const dLat = toRad(lat2 - lat1);
    const dLon = toRad(lng2 - lng1);
    const a = Math.sin(dLat/2) * Math.sin(dLat/2) + Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon/2) * Math.sin(dLon/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    return R * c;
  }

  private formatDistanceMeters(meters: number): string {
    if (meters >= 1000) return `${(meters/1000).toFixed(1)} كم`;
    return `${Math.round(meters)} م`;
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
        next: (response: any) => this.projects.set(response.items ?? []),
      });
  }

  loadBuildings(): void {
    this.buildingService
      .getBuildings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: any) => this.buildings.set(data ?? []),
      });
  }

  loadLookups(): void {
    this.lookupService
      .getFeatures(LookupStatus.All)
      .pipe(takeUntil(this.destroy$))
      .subscribe((data: any) => this.features.set((data ?? []).filter((f: any) => f.isActive)));


    this.lookupService
      .getInsurances(LookupStatus.All)
      .pipe(takeUntil(this.destroy$))
      .subscribe((data: any) => this.insurances.set((data ?? []).filter((g: any) => g.isActive)));

  }

  loadUnits(page: number): void {
    this.currentPage.set(page);
    this.isLoading.set(true);
    this.errorMessage.set('');

    const params: any = { page, pageSize: ADMIN_LIST_PAGE_SIZE };
    if (this.filterProjectId()) params.projectId = this.filterProjectId();
    if (this.filterBuildingId()) params.buildingId = this.filterBuildingId();

    this.unitService
      .getUnits(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          const pageData = normalizePagedResponse(response);
          // normalizePagedResponse returns unknown-typed items; cast to UnitCardModel[] for the signal
          this.units.set((pageData.items ?? []) as UnitCardModel[]);
          this.totalPages.set(pageData.totalPages);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('تعذر تحميل الوحدات.');
          this.isLoading.set(false);
        },
      });
  }

  onFilterProject(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const value = target.value ? Number(target.value) : null;
    this.filterProjectId.set(Number.isFinite(value) ? value : null);
    this.filterBuildingId.set(null);
    this.loadUnits(1);
  }

  onFilterBuilding(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const value = target.value ? Number(target.value) : null;
    this.filterBuildingId.set(Number.isFinite(value) ? value : null);
    this.loadUnits(1);
  }

  selectUnit(unit: UnitCardModel): void {
    this.isLoading.set(true);
    this.unitService
      .getUnit(unit.unitId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (details: any) => {
          const buildingId = details.buildingId || this.resolveBuildingIdForUnit(unit);
          this.selectedUnit.set({ ...unit, city: details.city ?? unit.city, region: details.region ?? unit.region, address: details.address ?? unit.address });
          this.unitForm.patchValue({
            name: details.name,
            status: details.status,
            buildingId,
            rooms: details.rooms ?? 1,
            salons: details.salons ?? 1,
            area: details.area ?? null,
            bathrooms: details.bathrooms ?? 1,
            floor: details.floor ?? 1,
            price: details.price ?? null,
            type: details.type,
            streetCount: details.streetCount ?? 1,
            // city/region derived from project; not patched into the form
            street: details.street ?? '',
            // remove project city from stored address to avoid duplication
            address: (details.address ?? '').toString().replace(new RegExp((details.city ?? '').toString(), 'ig'), '').trim(),
            latitude: details.latitude ?? null,
            longitude: details.longitude ?? null,
          });
          this.selectedFeatureIds.set(new Set(details.features.map((f: any) => f.featureId)));
          this.selectedInsuranceIds.set(new Set(details.insurance.map((i: any) => i.insuranceId)));
          this.nearbyFacilities.set(this.dedupeNearbyFacilities((details.nearbyFacilities ?? []).map((facility: any) => ({ ...facility, selected: true }))));
          this.selectedCity.set(details.city ?? '');
          this.mapQuery.set(details.city ?? '');
          this.mapCircleMeters.set(this.computeCircleRadiusMeters(details.area));
          this.selectedRegion.set(details.region ?? null);
          if (buildingId) {
            this.syncLocationFromBuilding(buildingId);
          }
          // run duplicate check so inline error appears immediately when editing
          this.checkUnitName();
          // check area limit for the loaded unit
          this.checkAreaLimit();
          this.resetMediaLabels();
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('تعذر تحميل بيانات الوحدة.' );
          this.isLoading.set(false);
        },
      });
  }

  private resolveBuildingIdForUnit(unit: UnitCardModel): number | null {
    const match = this.buildings().find(
      (building: AdminBuildingDto) => building.name === unit.buildingName && building.projectName === unit.projectName,
    );
    return match?.buildingId ?? null;
  }

  clearSelection(): void {
    this.selectedUnit.set(null);
    this.unitForm.reset({ type: 'Apartment', status: null, streetCount: 1 });
    this.selectedFeatureIds.set(new Set());
    this.selectedInsuranceIds.set(new Set());
    this.nearbyFacilities.set([]);
    this.selectedCity.set('');
    this.selectedRegion.set(null);
    this.mapQuery.set('');
    this.mapCircleMeters.set(null);
    this.resetMediaLabels();
  }

  private translateAdminError(message: string): string {
    const text = (message ?? '').toString();
    if (!text) {
      return 'تعذر تنفيذ العملية.';
    }

    const map: Array<[RegExp, string]> = [
      [/same name already exists/i, 'يوجد عنصر بنفس الاسم بالفعل.'],
      [/cannot move unit from sale path to rent path/i, 'لا يمكن نقل الوحدة من مسار البيع إلى مسار الإيجار.'],
      [/cannot move unit from rent path to sale path/i, 'لا يمكن نقل الوحدة من مسار الإيجار إلى مسار البيع.'],
      [/unit area/i, 'مساحة الوحدة غير صالحة أو تتجاوز الحد المسموح.'],
      [/floor/i, 'رقم الدور غير صالح أو يتجاوز عدد أدوار المبنى.'],
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

  private syncLocationFromBuilding(buildingId: number | null): void {
    if (!buildingId) {
      return;
    }

    const building = this.buildings().find((item: AdminBuildingDto) => item.buildingId === buildingId);
    if (!building) {
      return;
    }

    const project = this.projects().find((item: IProject) => item.projectId === building.projectId);
    if (!project) {
      return;
    }

    this.selectedCity.set(project.city ?? '');
    // mapQuery set to project city; city/region are not editable in this form
    this.mapQuery.set(project.city ?? '');
    this.mapCircleMeters.set(this.computeCircleRadiusMeters(project.landArea));
  }

  private computeCircleRadiusMeters(area?: number | null): number | null {
    if (area == null || area <= 0) {
      return null;
    }

    return Math.max(150, Math.min(2200, Math.sqrt(area / Math.PI)));
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

  // Manual add/update/remove of facilities is disabled. Facilities are populated automatically
  // from OpenStreetMap (Overpass) for the unit location within the project's circle.

  toggleFacilitySelection(index: number, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const items = this.nearbyFacilities();
    items[index] = { ...items[index], selected: checked };
    this.nearbyFacilities.set([...items]);
  }

  onThumbnailChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0] ?? null;

    if (file && !this.isValidImageFile(file)) {
      this.snackbar.error('يرجى اختيار صورة صالحة (JPG, PNG, WebP)');
      return;
    }

    this.unitForm.patchValue({ thumbnail: file });
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

    this.unitForm.patchValue({ images: files });
    this.imagesSummary.set(files?.length ? `${files.length} صورة` : 'لم يتم اختيار ملفات');
  }

  onDesignsChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const files = target.files ?? null;

    if (files) {
      const invalidFiles = Array.from(files).filter(f => !this.isValidImageFile(f));
      if (invalidFiles.length > 0) {
        this.snackbar.error('جميع التصاميم يجب أن تكون (JPG, PNG, WebP)');
        return;
      }
    }

    this.unitForm.patchValue({ designs: files });
    this.designsSummary.set(files?.length ? `${files.length} تصميم` : 'لم يتم اختيار ملفات');
  }

  onPanoramaChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0] ?? null;

    if (file && !this.isValidImageFile(file)) {
      this.snackbar.error('يرجى اختيار صورة صالحة (JPG, PNG, WebP)');
      return;
    }

    this.unitForm.patchValue({ panorama: file });
    this.panoramaName.set(file?.name ?? 'لم يتم اختيار ملف');
  }

  onVideoChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0] ?? null;

    if (file && !this.isValidVideoFile(file)) {
      this.snackbar.error('يرجى اختيار فيديو صالح (MP4, WebM, OGG)');
      return;
    }

    this.unitForm.patchValue({ video: file });
    this.videoName.set(file?.name ?? 'لم يتم اختيار ملف');
  }

  async submitForm(): Promise<void> {
    // ensure duplicate checks run before validity is evaluated
    this.checkUnitName();
    this.checkAreaLimit();
    if (this.unitForm.invalid) {
      this.unitForm.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();
    if (!payload) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const selected = this.selectedUnit();
    const request$ = selected
      ? this.unitService.updateUnit(selected.unitId, payload)
      : this.unitService.createUnit(payload);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.clearSelection();
        this.loadUnits(this.currentPage());
        this.snackbar.success('تم حفظ الوحدة بنجاح.');
      },
      error: (err: any) => {
        this.isSaving.set(false);
        const msg = this.translateAdminError(err?.error?.message || err?.message || 'تعذر حفظ الوحدة.');
        this.errorMessage.set(msg);
        this.snackbar.error(msg);
      },
    });
  }

  async deleteUnit(unit: UnitCardModel): Promise<void> {
    const confirmed = await this.alertService.confirm(`حذف الوحدة "${unit.name}"؟`);
    if (!confirmed) {
      return;
    }

    this.isSaving.set(true);
    this.unitService
      .deleteUnit(unit.unitId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.units.set(this.units().filter((item: UnitCardModel) => item.unitId !== unit.unitId));
          if (this.selectedUnit()?.unitId === unit.unitId) {
            this.clearSelection();
          }
          this.isSaving.set(false);
          this.snackbar.success('تم حذف الوحدة.');
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set('تعذر حذف الوحدة.');
          this.snackbar.error(this.errorMessage());
        },
      });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.loadUnits(page);
  }

  private buildPayload(): FormData | null {
    const values = this.unitForm.getRawValue();
    if (!values.name || !values.buildingId || values.price == null) {
      return null;
    }

    const payload = new FormData();
    payload.append('name', values.name);
    payload.append('buildingId', String(values.buildingId));
    payload.append('rooms', String(values.rooms ?? 1));
    payload.append('salons', String(values.salons ?? 1));
    payload.append('area', String(values.area ?? 0));
    payload.append('bathrooms', String(values.bathrooms ?? 1));
    payload.append('floor', String(values.floor ?? 1));
    payload.append('price', String(values.price));
    payload.append('type', values.type ?? 'Apartment');
    payload.append('status', values.status ?? 'Sale');
    payload.append('streetCount', String(values.streetCount ?? 1));

    // Append city and region from the selected building's project to ensure consistency
    const buildingId = Number(values.buildingId) || null;
    let projectCity = '';
    let projectRegion = '';
    if (buildingId) {
      const b = this.buildings().find((b: AdminBuildingDto) => b.buildingId === buildingId);
      if (b) {
        const proj = this.projects().find((p: IProject) => p.projectId === b.projectId);
        if (proj) {
          projectCity = proj.city ?? '';
          projectRegion = proj.region ?? '';
        }
      }
    }
    if (projectCity) payload.append('city', projectCity);
    // prefer region derived from map selection when present
    const regionToSend = this.selectedRegion() ?? projectRegion;
    if (regionToSend) payload.append('region', regionToSend);
    if (values.street) payload.append('street', values.street);
    if (values.address) payload.append('address', values.address);
    if (values.latitude != null) payload.append('latitude', String(values.latitude));
    if (values.longitude != null) payload.append('longitude', String(values.longitude));

    for (const id of this.selectedFeatureIds()) {
      payload.append('featureIds', String(id));
    }

    for (const id of this.selectedInsuranceIds()) {
      payload.append('insuranceIds', String(id));
    }

    let facilityIndex = 0;
    const dedupedFacilities = this.dedupeNearbyFacilities(this.nearbyFacilities());
    for (const facility of dedupedFacilities) {
      if (!facility || facility.selected === false || !facility.name?.trim()) {
        continue;
      }
      const prefix = `NearbyFacilities[${facilityIndex}]`;
      payload.append(`${prefix}.Name`, facility.name.trim());
      payload.append(`${prefix}.Type`, facility.type ?? 'Other');
      payload.append(`${prefix}.Distance`, facility.distance?.trim() || '0 م');
      payload.append(`${prefix}.Area`, String(facility.area ?? 0));
      if (facility.latitude != null) payload.append(`${prefix}.Latitude`, String(facility.latitude));
      if (facility.longitude != null) payload.append(`${prefix}.Longitude`, String(facility.longitude));
      facilityIndex += 1;
    }

    if (values.thumbnail) payload.append('thumbnailImage', values.thumbnail);
    if (values.images) {
      Array.from(values.images as FileList).forEach((file: File) => payload.append('images', file));
    }
    if (values.designs) {
      Array.from(values.designs as FileList).forEach((file: File) => payload.append('designs', file));
    }
    if (values.panorama) payload.append('panorama360', values.panorama);
    if (values.video) payload.append('videoFile', values.video);

    return payload;
  }

  private syncBuildingWithStatus(status: string | null): void {
    const buildingControl = this.unitForm.get('buildingId');
    const buildingId = Number(buildingControl?.value) || null;
    if (!status) {
      buildingControl?.patchValue(null, { emitEvent: false });
      return;
    }

    if (!buildingId) {
      return;
    }

    const building = this.buildings().find((item: AdminBuildingDto) => Number(item.buildingId) === buildingId);
    if (!building || !this.isBuildingCompatibleWithStatus(building, status)) {
      buildingControl?.patchValue(null, { emitEvent: false });
    }
  }

  private isBuildingCompatibleWithStatus(building: AdminBuildingDto, status: string | null | undefined): boolean {
    const isSalePath = status === 'Sale' || status === 'Sold';
    return isSalePath
      ? normalizeBuildingType(building.type) === 'Sale'
      : normalizeBuildingType(building.type) === 'Rent';
  }

  private dedupeNearbyFacilities(facilities: any[]): any[] {
    const seen = new Set<string>();
    const result: any[] = [];

    for (const facility of facilities ?? []) {
      if (!facility) {
        continue;
      }

      const name = String(facility.name ?? '').trim().toLowerCase();
      const type = String(facility.type ?? '').trim().toLowerCase();
      const lat = facility.latitude != null ? Number(facility.latitude).toFixed(6) : '';
      const lng = facility.longitude != null ? Number(facility.longitude).toFixed(6) : '';
      const distance = String(facility.distance ?? '').trim().toLowerCase();
      const key = `${name}|${type}|${lat}|${lng}|${distance}`;

      if (seen.has(key)) {
        continue;
      }

      seen.add(key);
      result.push(facility);
    }

    return result;
  }

  private resetMediaLabels(): void {
    this.thumbnailName.set('لم يتم اختيار ملف');
    this.imagesSummary.set('لم يتم اختيار ملفات');
    this.designsSummary.set('لم يتم اختيار ملفات');
    this.panoramaName.set('لم يتم اختيار ملف');
    this.videoName.set('لم يتم اختيار ملف');
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
