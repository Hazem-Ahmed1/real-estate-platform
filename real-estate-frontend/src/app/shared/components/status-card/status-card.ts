import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-card',
  imports: [],
  templateUrl: './status-card.html',
  styleUrl: './status-card.css',
})
export class StatusCard {
    item = input<any>();

}

