import { Component } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { HomeSectionHeaderWithFilters } from "../../../shared/components/home-section-header-with-filters/home-section-header-with-filters";
import { ProjectsList } from "../../../shared/components/projects-list/projects-list";
import { IProject } from '../../../models/IProject';
import { PaginationComponent } from "../../../shared/components/pagination/pagination";
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { Filters } from "../../../shared/components/filters/filters";

@Component({
  selector: 'app-projects',
  imports: [BreadcrumbComponent, ProjectsList, PaginationComponent, ContactSection, Filters],
  templateUrl: './projects.html',
  styleUrl: './projects.css',
})
export class Projects {
  projects: IProject[] = [
  {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخيل',
    type: 'للبيع',
    imageUrl: 'images/p1.jpg',
  },
  {
    title: 'مشروع الفيلاج 2',
    location: 'جدة - حي النخيل',
    type: 'للإيجار',
    imageUrl: 'images/p2.jpg',
  },
  {
    title: 'مشروع الفيلاج 3',
    location: 'جدة - حي النخيل',
    type: 'للبيع',
    imageUrl: 'images/p3.jpg',
  },
  {
    title: 'مشروع الفيلاج 4',
    location: 'جدة - حي النخيل',
    type: 'للإيجار',
    imageUrl: 'images/p1.jpg',
  },
  {
    title: 'مشروع الفيلاج 5',
    location: 'جدة - حي النخيل',
    type: 'للبيع',
    imageUrl: 'images/p2.jpg',
  },
  {
    title: 'مشروع الفيلاج 6',
    location: 'جدة - حي النخيل',
    type: 'للإيجار',
    imageUrl: 'images/p3.jpg',
  },
  {
    title: 'مشروع الفيلاج 7',
    location: 'جدة - حي النخيل',
    type: 'للبيع',
    imageUrl: 'images/p2.jpg',
  },
  {
    title: 'مشروع الفيلاج 8',
    location: 'جدة - حي النخيل',
    type: 'للإيجار',
    imageUrl: 'images/p1.jpg',
  },
  {
    title: 'مشروع الفيلاج 9',
    location: 'جدة - حي النخيل',
    type: 'للبيع',
    imageUrl: 'images/p3.jpg',
  },
  {
    title: 'مشروع الفيلاج 10',
    location: 'جدة - حي النخيل',
    type: 'للإيجار',
    imageUrl: 'images/p2.jpg',
  },
  {
    title: 'مشروع الفيلاج 11',
    location: 'جدة - حي النخيل',
    type: 'للبيع',
    imageUrl: 'images/p1.jpg',
  },
  {
    title: 'مشروع الفيلاج 12',
    location: 'جدة - حي النخيل',
    type: 'للإيجار',
    imageUrl: 'images/p3.jpg',
  }
];
currentPage = 1;

onPageChange(page: number) {
  this.currentPage = page;
}
}
