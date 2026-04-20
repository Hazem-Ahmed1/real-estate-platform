import { Component } from '@angular/core';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs';
import { BlogContent } from './blog-content/blog-content';

@Component({
  selector: 'app-blog',
  imports: [ContactSection, BreadcrumbComponent, BlogContent],
  templateUrl: './blog.html',
  styleUrl: './blog.css',
})
export class Blog {}
