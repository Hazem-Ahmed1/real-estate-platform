import { Component, Input, OnChanges, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IBlogPost } from '../../../../models/IBlogPost';

@Component({
  selector: 'app-blog-post',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './blog-post.html',
  styleUrl: './blog-post.css',
})
export class BlogPost implements OnChanges {
  @Input() post!: IBlogPost | null;

  readonly images = signal<string[]>([]);
  readonly currentImageIndex = signal(0);

  readonly currentImage = computed(() => {
    const list = this.images();
    const index = this.currentImageIndex();
    return list[index] || this.post?.image || '';
  });

  readonly hasMultipleImages = computed(() => this.images().length > 1);

  ngOnChanges() {
    this.syncImages();
  }

  private syncImages(): void {
    if (!this.post) {
      this.images.set([]);
      this.currentImageIndex.set(0);
      return;
    }

    const additionalImages = this.post.images?.filter(
      (img) => img !== this.post!.image
    ) ?? [];

    this.images.set([this.post.image, ...additionalImages]);
    this.currentImageIndex.set(0);
  }

  nextImage() {
    const list = this.images();
    if (!list.length) {
      return;
    }
    this.currentImageIndex.set((this.currentImageIndex() + 1) % list.length);
  }

  prevImage() {
    const list = this.images();
    if (!list.length) {
      return;
    }
    this.currentImageIndex.set(
      (this.currentImageIndex() - 1 + list.length) % list.length
    );
  }

  goToImage(index: number) {
    this.currentImageIndex.set(index);
  }
}
