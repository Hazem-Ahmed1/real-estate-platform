import { Component, input } from '@angular/core';
import { UnitCardModel } from '../../../models/IUnit';
import { UnitCard } from "../unit-card/unit-card";

@Component({
  selector: 'app-units-list',
  imports: [UnitCard],
  templateUrl: './units-list.html',
  styleUrl: './units-list.css',
})
export class UnitsList {
  units = input.required<UnitCardModel[]>();
}
