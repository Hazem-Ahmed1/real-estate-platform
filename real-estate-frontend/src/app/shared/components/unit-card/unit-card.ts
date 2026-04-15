import { DecimalPipe, NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type UnitCardModel = {
  title: string;
  location: string;
  price: string;
  type: string;
  imageURL: string;
  beds?: number;
  baths?: number;
  lounges?: number;
  area?: string;
  streetsText?: string;
};

@Component({
  selector: 'app-unit-card',
  imports: [NgOptimizedImage,DecimalPipe],
  templateUrl: './unit-card.html',
  styleUrl: './unit-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UnitCard {
  readonly unit = input.required<UnitCardModel>();

}
