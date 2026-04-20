import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UnitCardModel } from '../../../../models/IUnit';

@Component({
  selector: 'app-unit-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './unit-card.html',
  styleUrl: './unit-card.css',
})
export class UnitCard {
  @Input() unit!: UnitCardModel;
  @Output() cardClick = new EventEmitter<UnitCardModel>();

  onClick() {
    this.cardClick.emit(this.unit);
  }
}