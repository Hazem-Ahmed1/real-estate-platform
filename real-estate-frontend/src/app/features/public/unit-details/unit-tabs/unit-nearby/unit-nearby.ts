import { Component, input } from '@angular/core';

@Component({
  selector: 'app-unit-nearby',
  standalone: true,
  templateUrl: './unit-nearby.html',
  styleUrl: './unit-nearby.css',
})
export class UnitNearby {
  nearbyPlaces = input.required<{ name: string; distanceText: string }[]>();
}
