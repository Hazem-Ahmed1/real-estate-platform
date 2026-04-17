import { Component } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { Filters } from "../../../shared/components/filters/filters";
import { UnitsList } from "../../../shared/components/units-list/units-list";
import { PaginationComponent } from "../../../shared/components/pagination/pagination";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";

@Component({
  selector: 'app-units',
  imports: [BreadcrumbComponent, Filters, UnitsList, PaginationComponent, ContactSection],
  templateUrl: './units.html',
  styleUrl: './units.css',
})
export class Units {
    currentPage = 1;

onPageChange(page: number) {
  this.currentPage = page;
}

  units = [
  {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخبة',
    price: '58.000 ريال',
    beds: 4,
    lounges: 7,
    baths: 2,
    area: '148+148',
    streetsText: 'شارعين',
    type: 'للبيع',
    imageURL: 'images/p1.jpg'
  },
    {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخبة',
    price: '58.000 ريال',
    beds: 4,
    lounges: 7,
    baths: 2,
    area: '148+148',
    streetsText: 'شارعين',
    type: 'للبيع',
    imageURL: 'images/p1.jpg'
  }]
}
