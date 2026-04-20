import { Component } from '@angular/core';
import { IProject } from '../../../models/IProject';
import { SectionTitle } from '../../../shared/components/section-title/section-title';
import { ProjectGallery } from '../../../shared/components/project-gallery/project-gallery';
import { IconFeatureGrid } from '../../../shared/components/icon-feature-grid/icon-feature-grid';
import { ProjectMap } from '../../../shared/components/project-map/project-map';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs';
import { ProjectHero } from './project-hero/project-hero';
import { ProjectPanoramaSection } from './project-panorama-section/project-panorama-section';
import { ProjectVideoSection } from './project-video-section/project-video-section';

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [
    ContactSection,
    SectionTitle,
    ProjectGallery,
    IconFeatureGrid,
    ProjectMap,
    BreadcrumbComponent,
    ProjectHero,
    ProjectPanoramaSection,
    ProjectVideoSection,
  ],
  templateUrl: './project-details.html',
  styleUrl: './project-details.css',
})
export class ProjectDetails {

  // Data source: IProject
  project: IProject = {
    title: 'مشروع العزيزية 1',
    location: 'جدة - حي العزيزية',
    imageUrl: '/images/imgForFullProject.jpg',
    type: 'للبيع',
  };

  // Static data: stats (not in IProject)
  readonly stats = [
    { label: 'عدد الوحدات السكنية', value: 33, iconUrl: '/images/img_Units_builds.jpg' },
    { label: 'عدد المباني', value: 148, iconUrl: '/images/img_Units_builds.jpg' },
  ];

  // Static data: media (not in IProject)
  readonly panoramaImage = '/images/360_degree.png';
  readonly videoUrl = '/videos/hero.mp4';
  readonly videoPoster = '/images/imgForFullProject.jpg';
  readonly gallery = [
    '/images/PSide01.jpg',
    '/images/PSide02.jpg',
    '/images/PSide01.jpg',
    '/images/PSide02.jpg',
    '/images/PSide01.jpg',
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
    { label: 'ضمان شامل', durationText: '01 سنة' },
  ];

  // Resolved warranties: icon comes from map, not from data
  get resolvedWarranties() {
    return this.warrantyItems.map(w => ({
      label: w.label,
      iconClass: this.getWarrantyIcon(w.label),
      subLabel: w.durationText,
    }));
  }

  // Static data: map coordinates (not in IProject)
  readonly mapLat = 21.543333;
  readonly mapLng = 39.172779;
}
