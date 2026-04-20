import { Component } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";
import { CompanyAbout } from "./company-about/company-about";
import { CompanyVision } from "./company-vision/company-vision";
import { CompanyGoalsSection } from "./company-goals/company-goals-section";

@Component({
  selector: 'app-about-us',
  imports: [BreadcrumbComponent, ContactSection, CompanyAbout, CompanyVision, CompanyGoalsSection],
  templateUrl: './about-us.html',
  styleUrl: './about-us.css',
})
export class AboutUs {}
