



import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface BlogPost {
  id: number;
  image: string;
  date: string;
  title: string;
  excerpt: string;
}

@Component({
  selector: 'app-blog-content',
  imports: [],
  templateUrl: './blog-content.html',
  styleUrl: './blog-content.css',
})
export class BlogContent {
  blogPosts: BlogPost[] = [
    {
      id: 1,
      image:"images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 2,
      image: "/images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 3,
      image: "/images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 4,
      image: "/images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 5,
      image: "/images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 6,
      image: "/images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 7,
      image: "/images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 8,
      image: "/images/imgForFullProject.jpg",
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
    {
      id: 9,
      image: '/images/imgForFullProject.jpg',
      date: '07/24',
      title: '6 ملايين ريال مبيعات شركة منصات العقارية من مزاد « عبير الشمال »',
      excerpt: 'تطوير مشاريع عقارية متميزة تلي احتياجات السوق وتساهم في تحسين مستوى المعيشة للمجتمع...',
    },
  ];
}
