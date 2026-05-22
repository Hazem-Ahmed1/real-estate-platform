import { Component, input } from '@angular/core';
import { ISoldProject } from '../../../models/ISoldProject';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-sold-project-card',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './sold-project-card.html',
  styleUrl: './sold-project-card.css',
})
export class SoldProjectCard {
    project = input<ISoldProject>();
}
