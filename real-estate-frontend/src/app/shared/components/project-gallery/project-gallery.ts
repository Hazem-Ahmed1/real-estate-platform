import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-project-gallery',
  standalone: true,
  templateUrl: './project-gallery.html',
  styleUrls: ['./project-gallery.css']
})
export class ProjectGallery {

  @Input() images: string[] = [];

  onBackdropClick(event: MouseEvent) {
    const target = event.target as HTMLElement;

    // لو الضغط على صورة → لا تقفل
    if (target.closest('.gallery-modal-image')) return;

    // لو الضغط على زرار navigation → لا تقفل
    if (target.closest('.gallery-nav')) return;

    // لو الضغط على زرار close → لا تقفل (هو بيقفل لوحده)
    if (target.closest('.gallery-close-btn')) return;

    // غير كده → اقفل
    const modal = document.getElementById('galleryModal');
    if (!modal) return;

    const bsModal = (window as any).bootstrap?.Modal.getInstance(modal);
    bsModal?.hide();
  }
}
