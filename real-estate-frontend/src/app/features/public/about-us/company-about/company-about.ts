import { Component } from '@angular/core';
import { CompanyAboutImage } from './company-about-image/company-about-image';
import { CompanyAboutServices } from './company-about-services/company-about-services';

@Component({
  selector: 'company-about',
  standalone: true,
  imports: [CompanyAboutImage, CompanyAboutServices],
  templateUrl: './company-about.html',
  styleUrl: './company-about.css'
})
export class CompanyAbout {}