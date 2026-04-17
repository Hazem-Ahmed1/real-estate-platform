import { Component } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";

@Component({
  selector: 'app-contact',
  imports: [BreadcrumbComponent, ContactSection],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact {}
