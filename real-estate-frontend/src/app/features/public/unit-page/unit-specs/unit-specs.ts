import { Component, input } from '@angular/core';
import { UnitCardModel } from '../../../../models/IUnit';

@Component({
  selector: 'app-unit-specs',
  imports: [],
  templateUrl: './unit-specs.html',
  styleUrl: './unit-specs.css',
})
export class UnitSpecs {
    data = input.required<UnitCardModel>();
}
