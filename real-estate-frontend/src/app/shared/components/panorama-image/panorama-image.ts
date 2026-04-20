import { Component, input } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-panorama-image',
  standalone: true,
  imports: [NgOptimizedImage],
  templateUrl: './panorama-image.html',
  styleUrl: './panorama-image.css',
})
export class PanoramaImage {
  imageUrl = input.required<string>();
  altText  = input<string>('صورة بانورامية 360');
  sizes    = input<string>('(max-width: 767px) 100vw, 95vw');
}
