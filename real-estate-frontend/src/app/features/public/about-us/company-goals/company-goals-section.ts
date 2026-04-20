import { Component, signal } from '@angular/core';
import { CompanyGoalsCard } from './company-goals-card/company-goals-card';

interface Card {
  title: string;
  description: string;
  icon: string;
  highlighted?: boolean;
}
@Component({
  selector: 'app-company-goals-section',
  imports: [CompanyGoalsCard],
  templateUrl: './company-goals-section.html',
  styleUrl: './company-goals-section.css',
})
export class CompanyGoalsSection {
  activeIndex = signal<number>(0);

  setActive(index: number) {
    this.activeIndex.set(index);
  }

  cards = signal<Card[]>([
    {
      title: 'تقديم مشاريع عقارية مبتكرة',
      description: 'تطوير مشاريع عقارية متميزة تلبي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع..',
      icon: 'fa-building',
    },
    {
      title: 'تعزيز الشفافية والنزاهة',
      description: 'المحافظة على معايير عالية من الشفافية والنزاهة في جميع التعاملات لضمان الثقة والمصداقية..',
      icon: 'fa-comment-dots',
    },
    {
      title: 'الالتزام بالمعايير العالمية',
      description: 'تبني أفضل الممارسات والتقنيات العالمية في جميع مراحل التطوير العقاري لضمان الجودة والاستدامة..',
      icon: 'fa-globe',
    },
    {
      title: 'تعزيز رضا العملاء',
      description: 'ضمان تقديم خدمات عالية الجودة وتحقيق أعلى مستويات رضا العملاء من خلال التواصل المستمر..',
      icon: 'fa-users',
    },
  ]);
}
