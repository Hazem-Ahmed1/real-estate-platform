import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UnitCardModel } from '../../../../models/IUnit';
import { UnitCard } from '../unit-card/unit-card';

@Component({
  selector: 'app-unit-sidebar',
  standalone: true,
  imports: [CommonModule, UnitCard],
  templateUrl: './unit-sidebar.html',
  styleUrl: './unit-sidebar.css',
})
export class UnitSidebar {
  @Input() units: UnitCardModel[] = [];
  @Output() unitClick = new EventEmitter<UnitCardModel>();

  onUnitClick(unit: UnitCardModel) {
    this.unitClick.emit(unit);
  }
}