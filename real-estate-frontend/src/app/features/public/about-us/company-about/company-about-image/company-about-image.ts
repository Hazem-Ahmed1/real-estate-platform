import { Component } from '@angular/core';

@Component({
  selector: 'company-about-image',
  standalone: true,
  templateUrl: './company-about-image.html',
  styleUrl: './company-about-image.css'
})
export class CompanyAboutImage {
  imageUrl = '/images/aboutUs.jpg';
}