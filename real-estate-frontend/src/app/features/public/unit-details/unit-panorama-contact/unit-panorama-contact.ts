import { Component, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SectionTitle } from '../../../../shared/components/section-title/section-title';
import { PanoramaImage } from '../../../../shared/components/panorama-image/panorama-image';

@Component({
  selector: 'app-unit-panorama-contact',
  standalone: true,
  imports: [FormsModule, SectionTitle, PanoramaImage],
  templateUrl: './unit-panorama-contact.html',
  styleUrl: './unit-panorama-contact.css',
})
export class UnitPanoramaContact {
  panoramaImage = input.required<string>();

  submitForm(form: any) {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    console.log('Form Data:', form.value);

    // reset بعد الإرسال
    form.reset();
  }
}
