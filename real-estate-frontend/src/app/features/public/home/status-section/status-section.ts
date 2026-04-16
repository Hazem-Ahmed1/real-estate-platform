import { Component } from '@angular/core';

@Component({
  selector: 'app-status-section',
  templateUrl: './status-section.html',
  styleUrl: './status-section.css',
})
export class StatusSection {
  stats = [
  {
    number: '1159',
    label: 'مشروع سكني',
    imageURL: 'images/p1.jpg'
  },
  {
    number: '1,200',
    label: 'مساحات بناء',
    imageURL: 'images/p1.jpg'
  },
  {
    number: '472',
    label: 'وحدة سكنية',
    imageURL: 'images/p1.jpg'
  },
  {
    number: '148',
    label: 'بناية سكنية',
    imageURL: 'images/p1.jpg'
  }
];
}
