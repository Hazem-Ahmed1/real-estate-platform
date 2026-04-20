import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IBlogPost } from '../../../../models/IBlogPost';

@Component({
  selector: 'app-blog-post',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './blog-post.html',
  styleUrl: './blog-post.css',
})
export class BlogPost implements OnInit {
  @Input() post!: IBlogPost | null;

  images: string[] = [];
  currentImageIndex = 0;

  ngOnInit() {
  if (this.post) {
    const additionalImages = this.post.images?.filter(
      img => img !== this.post!.image  // exclude duplicate if post.image is already in images[]
    ) ?? [];

    this.images = [this.post.image, ...additionalImages];
  }
}

  get currentImage(): string {
    return this.images[this.currentImageIndex] || this.post?.image || '';
  }

  get hasMultipleImages(): boolean {
    return this.images.length > 1;
  }

  nextImage() {
    this.currentImageIndex = (this.currentImageIndex + 1) % this.images.length;
  }

  prevImage() {
    this.currentImageIndex =
      (this.currentImageIndex - 1 + this.images.length) % this.images.length;
  }

  goToImage(index: number) {
    this.currentImageIndex = index;
  }
}