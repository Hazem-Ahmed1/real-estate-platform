import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PublicBlogDto } from '../../../../models/PublicBlogDto';
import { BlogService } from '../../../../services/api/blog.service';

interface BlogCardView {
  id: number;
  image: string;
  date: string;
  title: string;
  excerpt: string;
}

@Component({
  selector: 'app-blog-content',
  imports: [DatePipe],
  templateUrl: './blog-content.html',
  styleUrl: './blog-content.css',
})
export class BlogContent implements OnInit {
  private readonly blogsService = inject(BlogService);
  private readonly router = inject(Router);

  readonly blogPosts = signal<BlogCardView[]>([]);
  readonly hasLoaded = signal(false);

  ngOnInit(): void {
    this.blogsService.getBlogs({ page: 1, pageSize: 12 }).subscribe({
      next: (response) => {
        this.blogPosts.set(this.mapToView(response.items ?? []));
        this.hasLoaded.set(true);
      },
      error: () => {
        this.blogPosts.set([]);
        this.hasLoaded.set(true);
      },
    });
  }

  blogDetaisl(id: number): void {
    this.router.navigate(['/blog', id]);
  }

  private mapToView(items: PublicBlogDto[]): BlogCardView[] {
    return items.map((item) => ({
      id: item.blogId,
      image: item.thumbnail?.imageUrl ?? item.images?.[0]?.imageUrl ?? '/images/imgForFullProject.jpg',
      date: item.publishDate ?? '',
      title: item.title,
      excerpt: this.stripHtml(item.description ?? ''),
    }));
  }

  private stripHtml(html: string): string {
    if (!html) {
      return '';
    }

    const doc = new DOMParser().parseFromString(html, 'text/html');
    return (doc.body.textContent ?? '').trim();
  }
}
