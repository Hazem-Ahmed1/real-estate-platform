import { Component } from '@angular/core';
import { CommonModule, NgFor } from '@angular/common';

@Component({
  selector: 'company-vision-gallery',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './company-vision-gallery.html',
  styleUrl: './company-vision-gallery.css'
})
export class CompanyVisionGallery {
  mainImage = '/images/vision03.jpg';
  thumbs = [
    '/images/vision02.jpg',
    '/images/vision01.jpg',
  ];
}