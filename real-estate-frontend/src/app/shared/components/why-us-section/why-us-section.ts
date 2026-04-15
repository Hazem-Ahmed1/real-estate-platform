import { Component, signal } from '@angular/core';
import { WhyUsCard } from "../why-us-card/why-us-card";
import { NgFor } from '@angular/common';

interface Card {
  title: string;
  description: string;
  icon: string;
  highlighted?: boolean;
}
@Component({
  selector: 'app-why-us-section',
  imports: [WhyUsCard,NgFor],
  templateUrl: './why-us-section.html',
  styleUrl: './why-us-section.css',
})
export class WhyUsSection {
    cards = signal<Card[]>([
    {
      title: 'التواصل معنا أينما كنت',
      description: 'فريقنا يدعمكم في جميع المجالات العقارية الاستثمارية، لنقدم لكم فرصًا مثالية.',
      icon: 'bi-chat-dots'
    },
    {
      title: 'خدمات متخصصة',
      description: 'نقدم خبرة في المجال العقاري الاستثماري لنضمن لكم أفضل الفرص.',
      icon: 'bi-pencil-square'
    },
    {
      title: 'عروض ومواقع حديثة',
      description: 'نوفر أحدث الفرص الاستثمارية المناسبة لكم.',
      icon: 'bi-broadcast'
    },
    {
      title: 'شركة رائدة في السوق',
      description: 'نحن خبراء في تقديم أفضل الفرص العقارية.',
      icon: 'bi-truck',
      highlighted: true
    }
  ]);
}
