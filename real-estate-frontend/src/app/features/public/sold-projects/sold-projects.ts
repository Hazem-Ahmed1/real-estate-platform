import { Component } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { HomeSectionHeaderWithFilters } from "../../../shared/components/home-section-header-with-filters/home-section-header-with-filters";
import { SoldProjectsList } from "../../../shared/components/sold-projects-list/sold-projects-list";
import { ISoldProject } from '../../../models/ISoldProject';
import { PaginationComponent } from "../../../shared/components/pagination/pagination";
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { Filters } from "../../../shared/components/filters/filters";

@Component({
  selector: 'app-sold-projects',
  imports: [BreadcrumbComponent, SoldProjectsList, PaginationComponent, ContactSection, Filters],
  templateUrl: './sold-projects.html',
  styleUrl: './sold-projects.css',
})
export class SoldProjects {

  currentPage = 1;

onPageChange(page: number) {
  this.currentPage = page;
}
    projects: ISoldProject[] = [
      {
        title: 'مشروع الفيلاج 1',
        location: 'جدة - حي النخبة',
        price: '1,200,000 ريال',
        imageURL: 'images/p1.jpg',
        beds: 4,
        baths: 3,
        lounges: 2,
        area: '148m²',
        units: 7,
        rooms: 4,
        streetsText: 'شارعين',
        type: 'تم البيع',
      },
      {
        title: 'مشروع الفيلاج 2',
        location: 'الرياض - حي الياسمين',
        price: '950,000 ريال',
        imageURL: 'images/p2.jpg',
        beds: 3,
        baths: 2,
        lounges: 1,
        area: '120m²',
        units: 5,
        rooms: 3,
        streetsText: 'شارع واحد',
        type: 'تم الإيجار',
      },
      {
        title: 'مشروع الفيلاج 3',
        location: 'الدمام - حي الشاطئ',
        price: '1,500,000 ريال',
        imageURL: 'images/p3.jpg',
        beds: 5,
        baths: 4,
        lounges: 2,
        area: '200m²',
        units: 10,
        rooms: 5,
        streetsText: 'ثلاث شوارع',
        type: 'تم الإيجار',
      },

    ];


}
