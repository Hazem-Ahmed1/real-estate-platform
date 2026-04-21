import { NgClass } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-why-us-card',
  imports: [NgClass],
  templateUrl: './why-us-card.html',
  styleUrl: './why-us-card.css',
})
export class WhyUsCard {
    @Input() data!: {
    title: string;
    description: string;
    icon: string;
    highlighted?: boolean;
  };
}
