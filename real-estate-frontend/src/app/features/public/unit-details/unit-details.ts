import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UnitCardModel } from '../../../models/IUnit';
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
export class UnitDetails {
  private readonly route = inject(ActivatedRoute);

  // Data source: UnitCardModel
  unit: UnitCardModel = {
    title: 'وحدة 1 / عمارة 1',
    type: 'للبيع',
    location: 'جدة - حي العزيزية',
    price: '200000',
    imageURL: '/images/imgForFullProject.jpg',
    beds: 4,
    baths: 7,
    lounges: 1,
    area: '1233*1248',
    streetsText: 'شارعين',
  };

  // Derived from UnitCardModel fields — label + value only, icons resolved in hero HTML via @switch
  get metadata() {
    return [
      { label: 'عدد الغرف', value: this.unit.beds?.toString() ?? '-' },
      { label: 'عدد الحمامات', value: this.unit.baths?.toString() ?? '-' },
      { label: 'المساحة', value: this.unit.area ?? '-' },
      // { label: 'الشارع', value: this.unit.streetsText ?? '-' },
      { label: '', value: this.unit.streetsText ?? '-' },

      // { label: 'عدد الصالات', value: this.unit.lounges?.toString() ?? '-' },
      { label: 'الدور', value:'01' },
    
    ];
  }

  // Static data: media (not in UnitCardModel)
  readonly panoramaImage = '/images/Unit_360_degree.png';
  readonly videoUrl = '/videos/hero.mp4';
  readonly videoPoster = '/images/imgForFullProject.jpg';
  readonly gallery = [
    '/images/PSide01.jpg',
    '/images/PSide02.jpg',
    '/images/PSide02.jpg',
    '/images/PSide02.jpg',
    '/images/PSide02.jpg',
  ];

  // Static data: design gallery (not in UnitCardModel)
  readonly designGallery = [
    // '/images/design_Image.png',
    // '/images/PSide01.jpg',
    // '/images/PSide02.jpg',
    // '/images/PSide01.jpg',
    // '/images/PSide02.jpg',

    '/images/p1.jpg',
    '/images/p2.jpg',
    '/images/p3.jpg',
    '/images/p1.jpg',
    '/images/p2.jpg',
  ];

  // ── Icon maps (centralized) ────────────────────────────────────────────────
  private readonly featureIconMap: Record<string, string> = {
    'خزان مستقل أرضي': 'fa-droplet',
    'خزان مستقل علوي': 'fa-water',
    'غرفة سائق': 'fa-car',
    'غرفة خادمة': 'fa-user',
  };

  private readonly warrantyIconMap: Record<string, string> = {
    'الهيكل الإنشائي': 'fa-arrows-rotate',
    'طبلون الكهرباء': 'fa-bolt',
    'عزل الخزانات': 'fa-faucet-drip',
    'عزل حراري': 'fa-temperature-half',
    'عزل مائي': 'fa-droplet',
    'أدوات صحية': 'fa-screwdriver-wrench',
    'المصعد': 'fa-building',
    'ضمان شامل': 'fa-shield-halved',
  };

  getFeatureIcon(label: string): string {
    return this.featureIconMap[label] ?? 'fa-circle-dot';
  }

  getWarrantyIcon(label: string): string {
    return this.warrantyIconMap[label] ?? 'fa-shield-halved';
  }
  // ──────────────────────────────────────────────────────────────────────────

  // Static data: feature labels only — no iconClass in data
  private readonly featureLabels = [
    'خزان مستقل أرضي',
    'خزان مستقل علوي',
    'غرفة سائق',
    'غرفة خادمة',
    'خزان مستقل أرضي',
    'خزان مستقل علوي',
    'غرفة سائق',
    // 'غرفة خادمة',
  ];

  // Resolved features: icon comes from map, not from data
  get resolvedFeatures() {
    return this.featureLabels.map(label => ({
      label,
      iconClass: this.getFeatureIcon(label),
    }));
  }

  // Static data: warranty labels + duration only — no iconClass in data
  private readonly warrantyItems = [
    { label: 'الهيكل الإنشائي', durationText: '+ 25 سنة' },
    { label: 'طبلون الكهرباء', durationText: '+ 25 سنة' },
    { label: 'عزل الخزانات', durationText: '+ 15 سنة' },
    { label: 'عزل حراري', durationText: '15 سنة' },
    { label: 'عزل مائي', durationText: '15 سنة' },
    { label: 'أدوات صحية', durationText: '02 سنة' },
    { label: 'المصعد', durationText: '02 سنة' },
    // { label: 'ضمان شامل', durationText: '01 سنة' },
  ];

  // Resolved warranties: icon comes from map, not from data
  get resolvedWarranties() {
    return this.warrantyItems.map(w => ({
      label: w.label,
      iconClass: this.getWarrantyIcon(w.label),
      subLabel: w.durationText,
    }));
  }

  // Static data: nearby places (not in UnitCardModel)
  readonly nearbyPlaces = [
    { name: 'مسجد سيدنا أبي بن عمر', distanceText: 'تبعد 1 كيلو' },
    { name: 'مستشفى الهلال', distanceText: 'تبعد 1 كيلو' },
    { name: 'مدرسة العلم والعلوم', distanceText: 'تبعد 1 كيلو' },
    // { name: 'مقهى النور', distanceText: 'تبعد 1 كيلو' },
  ];

  // Static data: map coordinates (not in UnitCardModel)
  readonly mapLat = 21.543333;
  readonly mapLng = 39.172779;
}
