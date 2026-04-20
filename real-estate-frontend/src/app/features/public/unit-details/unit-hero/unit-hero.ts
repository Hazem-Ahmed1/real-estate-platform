import { Component, input } from '@angular/core';

@Component({
  selector: 'app-unit-hero',
  standalone: true,
  imports: [],
  templateUrl: './unit-hero.html',
  styleUrl: './unit-hero.css',
})
export class UnitHero {
  // Data source: UnitCardModel.title
  title = input.required<string>();
  // Data source: UnitCardModel.location
  location = input.required<string>();
  // Derived from UnitCardModel fields (passed as static rows from parent)
  metadata = input.required<{ label: string; value: string }[]>();
}
