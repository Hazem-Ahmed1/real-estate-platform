import { DecimalPipe, NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { UnitCardModel } from '../../../models/IUnit';
import { RouterLink } from '@angular/router';


@Component({
  selector: 'app-unit-card',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './unit-card.html',
  styleUrl: './unit-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UnitCard {
  readonly unit = input.required<UnitCardModel>();

}
