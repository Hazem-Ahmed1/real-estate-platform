import { Component, inject, signal } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { IUnitDetails } from '../../../models/IUnitDetails';
import { SectionTitle } from '../../../shared/components/section-title/section-title';
import { ProjectVideoBlock } from '../../../shared/components/project-video-block/project-video-block';
import { ProjectGallery } from '../../../shared/components/project-gallery/project-gallery';
import { IconFeatureGrid } from '../../../shared/components/icon-feature-grid/icon-feature-grid';
import { ProjectMap } from '../../../shared/components/project-map/project-map';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { FormsModule } from '@angular/forms';

type UnitDetailsTab = 'features' | 'warranties' | 'designs' | 'nearby';

@Component({
  selector: 'app-unit-details',
  imports: [
    NgOptimizedImage,
    RouterLink,
    SectionTitle,
    ProjectVideoBlock,
    ProjectGallery,
    IconFeatureGrid,
    ProjectMap,
    ContactSection,
    FormsModule,
    CommonModule
  ],
  templateUrl: './unit-details.html',
  styleUrl: './unit-details.css',
})
export class UnitDetails {
  private readonly route = inject(ActivatedRoute);

  readonly activeTab = signal<UnitDetailsTab>('features');

  readonly unitDetails: IUnitDetails = {
    unitName:  'وحدة / عمارة 1',
    unitType: 'متاح للبيع (تملك الآن)',
    locationText: 'جدة - حي العزيزية',
    priceText: '200,000 رس',
    metadata: [
      { label: 'عدد الغرف', value: '04', iconClass: 'fa-bed' },
      { label: 'عدد الحمامات', value: '07', iconClass: 'fa-bath' },
      { label: 'المساحة', value: '1233*1248', iconClass: 'fa-ruler-combined' },
      { label: 'شارعين', value: '', iconClass: 'fa-road' },
      { label: 'الدور', value: '01', iconClass: 'fa-layer-group' },
    ],
    media: {
      panoramaImage: '/images/Unit_360_degree.png',
      videoUrl: '/videos/hero.mp4',
      // videoUrl: '/videos/try.mkv',
      videoPoster: '/images/imgForFullProject.jpg',
      gallery: [
        '/images/PSide01.jpg',
        '/images/PSide02.jpg',
        '/images/PSide02.jpg',
        '/images/PSide02.jpg',
        '/images/PSide02.jpg',
      ],
      designGallery: [
        '/images/design_Image.png',
        '/images/PSide02.jpg',
        '/images/PSide02.jpg',
        '/images/PSide02.jpg',
        '/images/PSide02.jpg',
      ],
    },
    features: [
      { label: 'خزان مستقل أرضي', iconClass: 'fa-shield-halved' },
      { label: 'خزان مستقل علوي', iconClass: 'fa-shield-halved' },
      { label: 'غرفة سائق', iconClass: 'fa-shield-halved' },
      { label: 'غرفة خادمة', iconClass: 'fa-shield-halved' },
      { label: 'خزان مستقل أرضي', iconClass: 'fa-shield-halved' },
      { label: 'خزان مستقل علوي', iconClass: 'fa-shield-halved' },
      { label: 'غرفة سائق', iconClass: 'fa-shield-halved' },
      { label: 'غرفة خادمة', iconClass: 'fa-shield-halved' },
    ],
    warranties: [
      { label: 'الهيكل الإنشائي', durationText: '+ 25 سنة', iconClass: 'fa-arrows-rotate' },
      { label: 'طبلون الكهرباء', durationText: '+ 25 سنة', iconClass: 'fa-bolt' },
      { label: 'عزل الخزانات', durationText: '+ 15 سنة', iconClass: 'fa-faucet-drip' },
      { label: 'عزل حراري', durationText: '15 سنة', iconClass: 'fa-temperature-half' },
      { label: 'عزل مائي', durationText: '15 سنة', iconClass: 'fa-droplet' },
      { label: 'أدوات صحية', durationText: '02 سنة', iconClass: 'fa-screwdriver-wrench' },
      { label: 'المصعد', durationText: '02 سنة', iconClass: 'fa-building' },
      { label: 'ضمان شامل', durationText: '01 سنة', iconClass: 'fa-shield-halved' },
    ],
    nearbyPlaces: [
      { name: 'مسجد سيدنا أبي بن عمر', distanceText: 'تبعد 1 كيلو' },
      { name: 'مستشفى الهلال', distanceText: 'تبعد 1 كيلو' },
      { name: 'مدرسة العلم والعلوم', distanceText: 'تبعد 1 كيلو' },
      { name: 'مقهى النور', distanceText: 'تبعد 1 كيلو' },
    ],
    map: {
      lat: 21.543333,
      lng: 39.172779,
    },
  };

  get mappedWarranties() {
    return this.unitDetails.warranties.map((item) => ({
      label: item.label,
      iconClass: item.iconClass,
      subLabel: item.durationText,
    }));
  }

  setTab(tab: UnitDetailsTab): void {
    this.activeTab.set(tab);
  }

  private routeUnitName(): string | undefined {
    const raw = this.route.snapshot.paramMap.get('unitName')?.trim();
    if (!raw) {
      return undefined;
    }

    return decodeURIComponent(raw);
  }

  submitForm(form: any) {
  if (form.valid) {
    console.log('Form Data:', form.value);

    // reset بعد الإرسال
    form.reset();
  }
}
}
