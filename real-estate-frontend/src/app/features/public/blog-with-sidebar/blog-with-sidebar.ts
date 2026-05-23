import { Component, DestroyRef, EventEmitter, OnInit, Output, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { catchError, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";
import { IBlogPost } from '../../../models/IBlogPost';
import { UnitCardModel } from '../../../models/IUnit';
import { BlogPost } from "./blog-post/blog-post";
import { UnitSidebar } from "./unit-sidebar/unit-sidebar";
import { BlogService } from '../../../services/api/blog.service';
import { UnitService } from '../../../services/api/unit.service';
import { PublicBlogDto } from '../../../models/PublicBlogDto';

@Component({
  selector: 'app-blog-with-sidebar',
  imports: [BreadcrumbComponent, ContactSection, BlogPost, UnitSidebar],
  templateUrl: './blog-with-sidebar.html',
  styleUrl: './blog-with-sidebar.css',
})
export class BlogWithSidebar implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly blogsService = inject(BlogService);
  private readonly unitService = inject(UnitService);
  private readonly route = inject(ActivatedRoute);
  private readonly selectedBlogId = signal(0);

  readonly post = signal<IBlogPost | null>(null);
  readonly units = signal<UnitCardModel[]>([]);

  @Output() unitClick = new EventEmitter<UnitCardModel>();

  onUnitClick(unit: UnitCardModel) {
    this.unitClick.emit(unit);
  }

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        switchMap((params) => {
          const id = Number(params.get('id'));
          const blogId = Number.isFinite(id) ? id : 0;
          if (!blogId) {
            return this.blogsService.getBlogs({ page: 1, pageSize: 1 });
          }
          return this.blogsService.getBlog(blogId).pipe(
            catchError(() => this.blogsService.getBlogs({ page: 1, pageSize: 1 }))
          );
        })
      )
      .subscribe((response) => {
        console.info('Blog details response', response);
        if ('items' in response) {
          const first = response.items?.[0];
          this.post.set(first ? this.mapBlogToPost(first) : null);
        } else {
          this.post.set(this.mapBlogToPost(response));
        }
        this.selectedBlogId.set(this.post()?.id ?? 0);
      });

    this.units.set([]);
    this.loadLatestUnits();
  }

  private loadLatestUnits(): void {
    this.unitService
      .getUnits({ page: 1, pageSize: 8 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => this.units.set(response.items ?? []),
        error: () => this.units.set([]),
      });
  }

  private mapBlogToPost(blog: PublicBlogDto): IBlogPost {
    return {
      id: blog.blogId,
      title: blog.title,
      date: blog.publishDate ?? '',
      image: blog.thumbnail?.imageUrl ?? blog.images?.[0]?.imageUrl ?? '/images/imgForFullProject.jpg',
      images: (blog.images ?? []).map((img) => img.imageUrl),
      excerpt: this.stripHtml(blog.description ?? ''),
      sections: [
        {
          content: this.splitParagraphs(blog.description ?? ''),
        },
      ],
    };
  }


  private splitParagraphs(html: string): string[] {
    if (!html) {
      return [];
    }

    const doc = new DOMParser().parseFromString(html, 'text/html');
    const paragraphs = Array.from(doc.querySelectorAll('p')).map(
      (p) => p.innerHTML.trim()
    );
    if (paragraphs.length > 0) {
      return paragraphs.filter(Boolean);
    }

    const text = doc.body.innerHTML.trim();
    return text ? [text] : [];
  }

  private stripHtml(html: string): string {
    if (!html) {
      return '';
    }

    const doc = new DOMParser().parseFromString(html, 'text/html');
    return (doc.body.textContent ?? '').trim();
  }

}
