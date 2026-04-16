import { DecimalPipe, NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { UnitCardModel } from '../../../models/IUnit';


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
