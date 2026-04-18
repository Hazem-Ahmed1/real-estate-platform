import { Component } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { IProjectDetails } from '../../../models/IProjectDetails';
import { SectionTitle } from '../../../shared/components/section-title/section-title';
import { ProjectStatCard } from '../../../shared/components/project-stat-card/project-stat-card';
import { ProjectVideoBlock } from '../../../shared/components/project-video-block/project-video-block';
import { ProjectGallery } from '../../../shared/components/project-gallery/project-gallery';
import { IconFeatureGrid } from '../../../shared/components/icon-feature-grid/icon-feature-grid';
import { ProjectMap } from '../../../shared/components/project-map/project-map';
import { RouterLink } from '@angular/router';
import { TopNavbar } from "../../../shared/components/top-navbar/top-navbar";
import { Navbar } from "../../../shared/components/navbar/navbar";
import { Footer } from "../../../shared/components/footer/footer";

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [
    NgOptimizedImage,
    ContactSection,
    SectionTitle,
    ProjectStatCard,
    ProjectVideoBlock,
    ProjectGallery,
    IconFeatureGrid,
    ProjectMap,
    RouterLink,
    TopNavbar,
    Navbar,
    Footer
],
  templateUrl: './project-details.html',
  styleUrl: './project-details.css',
})
export class ProjectDetails {

  projectDetails: IProjectDetails = {
    projectName: 'مشروع العزيزية 1',
    projectType: 'متاح للبيع (تملك الآن)',
    locationText: 'الرياض - حي المعذر الشمالي',
    stats: [
      { label: 'عدد الوحدات السكنية', value: 33, iconUrl: '/images/img_Units_builds.jpg' },
      { label: 'عدد المباني', value: 148, iconUrl: '/images/img_Units_builds.jpg' },
    ],
    media: {
      panoramaImage: '/images/360_degree.png',
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
    map: {
      lat: 21.543333,
      lng: 39.172779,
    },
  };

  get mappedWarranties() {
    return this.projectDetails.warranties.map((w) => ({
      label: w.label,
      iconClass: w.iconClass,
      subLabel: w.durationText,
    }));
  }
}
