import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-project-gallery',
  standalone: true,
  templateUrl: './project-gallery.html',
  styleUrls: ['./project-gallery.css']
})
export class ProjectGallery {

  @Input() images: string[] = [];

  readonly uniqueId = Math.random().toString(36).substring(2, 9);
  readonly modalId = `galleryModal_${this.uniqueId}`;
  readonly carouselId = `carouselGallery_${this.uniqueId}`;

  currentIndex: number = 0;
  isZoomed: boolean = false;

  // openCarousel(index: number) {
  //   this.currentIndex = index;
  //   this.isZoomed = false; // reset zoom

  //   setTimeout(() => {
  //     const carousel = document.getElementById('carouselGallery');
  //     if (!carousel) return;

  //     const bsCarousel =
  //       (window as any).bootstrap?.Carousel.getInstance(carousel) ||
  //       new (window as any).bootstrap.Carousel(carousel);

  //     bsCarousel.to(index);
  //   });
  // }

  openCarousel(index: number) {
  this.currentIndex = index;
  this.isZoomed = false;

  const modalEl = document.getElementById(this.modalId);
  if (!modalEl) return;

  // لما المودال يفتح فعليًا
  modalEl.addEventListener('shown.bs.modal', () => {
    const carousel = document.getElementById(this.carouselId);
    if (!carousel) return;

    const bsCarousel =
      (window as any).bootstrap.Carousel.getOrCreateInstance(carousel);

    bsCarousel.to(index);
  }, { once: true });
}
  toggleZoom() {
    this.isZoomed = !this.isZoomed;
  }

  onBackdropClick(event: MouseEvent) {
    const target = event.target as HTMLElement;

    if (target.closest('.gallery-modal-image')) return;
    if (target.closest('.gallery-nav')) return;
    if (target.closest('.gallery-close-btn')) return;

    const modal = document.getElementById(this.modalId);
    if (!modal) return;

    const bsModal = (window as any).bootstrap?.Modal.getInstance(modal);
    bsModal?.hide();
  }
}