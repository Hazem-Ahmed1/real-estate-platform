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
  mainImage = 'https://images.unsplash.com/photo-1460317442991-0ec209397118?w=600&q=80';
  thumbs = [
    'https://images.unsplash.com/photo-1444653614773-995cb1ef9efa?w=400&q=80',
    'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&q=80'
  ];
}