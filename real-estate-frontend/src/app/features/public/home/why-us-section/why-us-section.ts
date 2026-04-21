import { Component, signal } from '@angular/core';
import { WhyUsCard } from '../../../../shared/components/why-us-card/why-us-card';

interface Card {
  title: string;
  description: string;
  icon: string;
  highlighted?: boolean;
}
@Component({
  selector: 'app-why-us-section',
  imports: [WhyUsCard],
  templateUrl: './why-us-section.html',
  styleUrl: './why-us-section.css',
})
export class WhyUsSection {
  cards = signal<Card[]>([
    {
      title: 'شركة رائدة في السوق',
      description: 'مرحبًا بكم في منصتنا العقارية الاستثمارية الرائدة، حيث نقدم لكم فرصة مثالية...',
      icon: 'fa-comment-dots',
      highlighted: true,
    },
    {
      title: 'التواصل معنا أينما كنت',
      description: 'فريقنا يدعمكم في جميع المجالات العقارية الاستثمارية، لنقدم لكم فرصًا مثالية.',
      icon: 'fa-web-awesome',
    },
    {
      title: 'خدمات متخصصة',
      description: 'نقدم خبرة في المجال العقاري الاستثماري لنضمن لكم أفضل الفرص.',
      icon: 'fa-location-crosshairs',
    },
    {
      title: 'عروض ومواقع حديثة',
      description: 'نوفر أحدث الفرص الاستثمارية المناسبة لكم.',
      icon: 'fa-rectangle-list',
    },
  ]);
}
