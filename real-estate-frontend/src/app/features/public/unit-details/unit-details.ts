import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UnitCardModel, IUnitDetails } from '../../../models/IUnit';
import { UnitService } from '../../../services/api/unit.service';
import { SectionTitle } from '../../../shared/components/section-title/section-title';
import { ProjectGallery } from '../../../shared/components/project-gallery/project-gallery';
import { ProjectMap } from '../../../shared/components/project-map/project-map';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs';
import { UnitHero } from './unit-hero/unit-hero';
import { UnitPanoramaContact } from './unit-panorama-contact/unit-panorama-contact';
import { UnitVideoSection } from './unit-video-section/unit-video-section';
import { UnitTabs } from './unit-tabs/unit-tabs';

@Component({
  selector: 'app-unit-details',
  standalone: true,
  imports: [
    SectionTitle,
    ProjectGallery,
    ProjectMap,
    ContactSection,
    BreadcrumbComponent,
    UnitHero,
    UnitPanoramaContact,
    UnitVideoSection,
    UnitTabs,
  ],
  templateUrl: './unit-details.html',
  styleUrl: './unit-details.css',
})
export class UnitDetails implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly unitService = inject(UnitService);
  private readonly router = inject(Router);
  private sub?: Subscription;

  readonly unit = signal<IUnitDetails | null>(null);

  get metadata() {
    const u = this.unit();
    if (!u) return [];
    return [
      { label: 'عدد الغرف',    value: (u.rooms ?? 0).toString() },
      { label: 'المساحة',      value: `${u.area ?? 0} * ${u.area ?? 0}` },
      { label: 'عدد الشوارع',  value: `${u.streetCount ?? 0}` },
      { label: 'عدد الحمامات', value: (u.bathrooms ?? 0).toString() },
      { label: 'الدور',        value: (u.floor ?? 0).toString().padStart(2, '0') },
    ];
  }

  get gallery() {
    return this.unit()?.images ?? [];
  }

  get designGallery() {
    return this.unit()?.designs ?? [];
  }

  get resolvedFeatures() {
    const u = this.unit();
    if (!u?.features) return [];
    return u.features.map(f => ({
      label: f.name,
      iconClass: 'fa-circle-check',
    }));
  }

  get resolvedWarranties() {
    const u = this.unit();
    if (!u?.insurance) return [];
    return u.insurance.map(i => ({
      label: i.name,
      iconClass: 'fa-shield-halved',
      subLabel: this.formatWarrantyDuration(i.duration),
    }));
  }

  private formatWarrantyDuration(duration?: number): string {
    const value = duration ?? 0;
    return `+ ${value.toString().padStart(2, '0')} سنة`;
  }

  private getNearbyFacilityIcon(type: string): string {
    const t = String(type).toLowerCase();
    if (t.includes('mosque') || t.includes('مسجد')) return 'fa-mosque';
    if (t.includes('school') || t.includes('مدرسة') || t.includes('جامعة')) return 'fa-graduation-cap';
    if (t.includes('hospital') || t.includes('مستشفى') || t.includes('عيادة')) return 'fa-hospital';
    if (t.includes('restaurant') || t.includes('مطعم') || t.includes('اكل')) return 'fa-utensils';
    if (t.includes('park') || t.includes('حديقة') || t.includes('منتزه')) return 'fa-tree';
    if (t.includes('bank') || t.includes('بنك') || t.includes('صراف')) return 'fa-building-columns';
    if (t.includes('pharmacy') || t.includes('صيدلية')) return 'fa-prescription-bottle-medical';
    if (t.includes('supermarket') || t.includes('سوبر ماركت') || t.includes('بقال')) return 'fa-cart-shopping';
    if (t.includes('club') || t.includes('نادي') || t.includes('رياضة')) return 'fa-dumbbell';
    return 'fa-location-dot';
  }

  get nearbyPlaces() {
    const u = this.unit();
    if (!u?.nearbyFacilities) return [];
    const seen = new Set<string>();
    const uniqueFacilities = u.nearbyFacilities.filter(n => {
      const key = [
        String(n.name ?? '').trim().toLowerCase(),
        String(n.type ?? '').trim().toLowerCase(),
        String(n.distance ?? '').trim().toLowerCase(),
        n.latitude != null ? Number(n.latitude).toFixed(6) : '',
        n.longitude != null ? Number(n.longitude).toFixed(6) : '',
      ].join('|');

      if (seen.has(key)) {
        return false;
      }

      seen.add(key);
      return true;
    });

    return uniqueFacilities.map(n => ({
      name: n.name,
      distanceText: `تبعد ${n.distance}`,
      iconClass: this.getNearbyFacilityIcon(n.type),
    }));
  }

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe(params => {
      const rawId = params.get('id');
      const id = rawId ? Number(rawId) : NaN;
      if (!Number.isFinite(id)) {
        this.router.navigate(['/']);
        return;
      }

      this.unit.set(null);

      this.unitService.getUnit(id).subscribe({
        next: (unit) => {
          if (!unit) {
            this.router.navigate(['/']);
            return;
          }
          this.unit.set(unit);
        },
        error: () => {
          this.router.navigate(['/']);
        }
      });
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
