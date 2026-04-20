import { NgClass } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-company-goals-card',
  imports: [NgClass],
  templateUrl: './company-goals-card.html',
  styleUrl: './company-goals-card.css',
})
export class CompanyGoalsCard {
  @Input() data!: {
    title: string;
    description: string;
    icon: string;
  };
  @Input() isActive: boolean = false;
}
