import { Component, input } from '@angular/core';

@Component({
  selector: 'app-sold-project-card',
  imports: [],
  templateUrl: './sold-project-card.html',
  styleUrl: './sold-project-card.css',
})
export class SoldProjectCard {
    project = input<any>();

}
