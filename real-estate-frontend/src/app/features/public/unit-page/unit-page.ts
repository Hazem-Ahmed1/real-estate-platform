import { Component } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";
import { UnitSpecs } from "./unit-specs/unit-specs";
import { UnitCardModel } from '../../../models/IUnit';
import { FormCard } from "./form-card/form-card";
import { ImageCard } from "./image-card/image-card";

@Component({
  selector: 'app-unit-page',
  imports: [BreadcrumbComponent, ContactSection, UnitSpecs, FormCard, ImageCard],
  templateUrl: './unit-page.html',
  styleUrl: './unit-page.css',
})
export class UnitPage {

 unit: UnitCardModel = {
  title: 'وحدة / عمارة',
  location: 'جدة - حي العزيزية',
  price: '200,000 ر.س',
  type: 'متاح للبيع',
  imageURL: 'images/p1.jpg',

  beds: 4,
  baths: 3,
  lounges: 2,
  area: '148 م²',
  streetsText: 'شارعين'
};


}
