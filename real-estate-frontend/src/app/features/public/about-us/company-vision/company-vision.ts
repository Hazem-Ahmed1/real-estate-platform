import { Component } from '@angular/core';
import { CompanyVisionText } from './company-vision-text/company-vision-text';
import { CompanyVisionGallery } from './company-vision-gallery/company-vision-gallery';

@Component({
  selector: 'company-vision',
  standalone: true,
  imports: [CompanyVisionText, CompanyVisionGallery],
  templateUrl: './company-vision.html',
  styleUrl: './company-vision.css'
})
export class CompanyVision {}