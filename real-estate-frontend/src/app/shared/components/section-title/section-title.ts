import { Component, input } from '@angular/core';

@Component({
  selector: 'app-section-title',
  standalone: true,
  templateUrl: './section-title.html',
  styleUrl: './section-title.css',
})
export class SectionTitle {
  subtitle = input.required<string>();
  title = input.required<string|null>();
}
