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
  unitId: 1,
  name: 'وحدة / عمارة',
  address: 'جدة - حي العزيزية',
  price: 200000,
  status: 'Sale',
  type: 'Apartment',
  thumbnailUrl: 'images/p1.jpg',
  rooms: 4,
  salons: 2,
  area: 148,
  streetCount: 2,
  projectName: 'مشروع العزيزية',
  buildingName: 'عمارة 1',
  city: 'جدة',
  region: 'العزيزية'
};


}
