import { Component, input } from '@angular/core';
import { ISoldProject } from '../../../models/ISoldProject';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-sold-project-card',
  imports: [DecimalPipe],
  templateUrl: './sold-project-card.html',
  styleUrl: './sold-project-card.css',
})
export class SoldProjectCard {
    project = input<ISoldProject>();
}
