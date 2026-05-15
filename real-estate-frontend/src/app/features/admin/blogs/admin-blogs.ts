import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
  signal,
  SecurityContext,
} from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { AdminBlogDto } from '../../../models/AdminBlogDto';
import { BlogService } from '../../../services/api/blog.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';

@Component({
  selector: 'app-admin-blogs',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './admin-blogs.html',
  styleUrl: './admin-blogs.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminBlogs implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly fb = inject(FormBuilder);
  private readonly blogsService = inject(BlogService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly snackbar = inject(SnackbarService);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');
  readonly blogs = signal<AdminBlogDto[]>([]);
  readonly selectedBlog = signal<AdminBlogDto | null>(null);
  readonly previewHtml = signal('');
  readonly thumbnailName = signal('لم يتم اختيار ملف');
  readonly imagesSummary = signal('لم يتم اختيار ملفات');

  readonly blogForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(150)]],
    description: [''],
    publishDate: [''],
    thumbnail: [null as File | null],
    images: [null as FileList | null],
  });

  @ViewChild('descriptionEditor')
  private readonly descriptionEditor?: ElementRef<HTMLDivElement>;

  ngOnInit(): void {
    this.loadBlogs();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBlogs(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.blogsService
      .getBlogs({ page: 1, pageSize: 20 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.blogs.set(response.items ?? []);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.getBlogErrorMessage(error, 'load'));
          this.isLoading.set(false);
        },
      });
  }

  selectBlog(blog: AdminBlogDto): void {
    this.selectedBlog.set(blog);
    this.blogForm.patchValue({
      title: blog.title,
      description: blog.description ?? '',
      publishDate: blog.publishDate ? blog.publishDate.substring(0, 10) : '',
    });
    this.syncEditorWithForm();
    this.previewHtml.set(this.buildPreviewHtml(blog.description ?? ''));
  }

  clearSelection(): void {
    this.selectedBlog.set(null);
    this.blogForm.reset();
    this.syncEditorWithForm();
    this.previewHtml.set('');
    this.thumbnailName.set('لم يتم اختيار ملف');
    this.imagesSummary.set('لم يتم اختيار ملفات');
  }

  onThumbnailChange(event: Event): void {
    const target = event.target;

    if (!(target instanceof HTMLInputElement)) {
      return;
    }

    const file = target.files?.[0] ?? null;
    this.blogForm.patchValue({ thumbnail: file });
    this.thumbnailName.set(file?.name ?? 'لم يتم اختيار ملف');
  }

  onImagesChange(event: Event): void {
    const target = event.target;

    if (!(target instanceof HTMLInputElement)) {
      return;
    }

    const files = target.files ?? null;
    this.blogForm.patchValue({ images: files });
    if (!files || files.length === 0) {
      this.imagesSummary.set('لم يتم اختيار ملفات');
      return;
    }
    this.imagesSummary.set(`تم اختيار ${files.length} صورة`);
  }

  submitForm(): void {
    if (this.blogForm.invalid) {
      this.blogForm.markAllAsTouched();
      return;
    }

    const payload = new FormData();
    const { title, description, publishDate, thumbnail, images } = this.blogForm.getRawValue();

    payload.append('title', title ?? '');
    if (description) {
      payload.append('description', description);
    }
    if (publishDate) {
      payload.append('publishDate', publishDate);
    }
    if (thumbnail) {
      payload.append('thumbnail', thumbnail);
    }
    if (images) {
      Array.from(images).forEach((file) => payload.append('images', file));
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const selected = this.selectedBlog();
    const request$ = selected
      ? this.blogsService.updateBlog(selected.blogId, payload)
      : this.blogsService.createBlog(payload);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (blog) => {
        if (selected) {
          this.blogs.set(
            this.blogs().map((item) => (item.blogId === blog.blogId ? blog : item))
          );
        } else {
          this.blogs.set([blog, ...this.blogs()]);
        }
        this.isSaving.set(false);
        this.clearSelection();
        this.snackbar.show('تم حفظ المدونة بنجاح.', 'success');
      },
      error: (error: unknown) => {
        this.errorMessage.set(this.getBlogErrorMessage(error, 'save'));
        this.isSaving.set(false);
        this.snackbar.show(this.errorMessage(), 'error');
      },
    });
  }

  onDescriptionInput(): void {
    const editor = this.descriptionEditor?.nativeElement;
    if (!editor) {
      return;
    }

    const html = editor.innerHTML;
    this.blogForm.patchValue({ description: html });
    this.previewHtml.set(this.buildPreviewHtml(html));
  }

  applyFormat(command: 'bold' | 'italic' | 'removeFormat'): void {
    document.execCommand(command);
    this.onDescriptionInput();
  }

  applyColor(event: Event): void {
    const target = event.target;
    if (!(target instanceof HTMLInputElement)) {
      return;
    }

    document.execCommand('foreColor', false, target.value);
    this.onDescriptionInput();
  }

  insertLink(): void {
    const url = window.prompt('ادخل رابط URL');
    if (!url) {
      return;
    }

    document.execCommand('createLink', false, url.trim());
    this.onDescriptionInput();
  }

  focusEditor(): void {
    this.descriptionEditor?.nativeElement.focus();
  }

  private syncEditorWithForm(): void {
    const editor = this.descriptionEditor?.nativeElement;
    if (!editor) {
      return;
    }

    const html = this.blogForm.controls.description.value ?? '';
    editor.innerHTML = html;
    this.previewHtml.set(this.buildPreviewHtml(html));
  }

  deleteBlog(blog: AdminBlogDto): void {
    this.isSaving.set(true);
    this.errorMessage.set('');

    this.blogsService
      .deleteBlog(blog.blogId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.blogs.set(this.blogs().filter((item) => item.blogId !== blog.blogId));
          if (this.selectedBlog()?.blogId === blog.blogId) {
            this.clearSelection();
          }
          this.isSaving.set(false);
          this.snackbar.show('تم حذف المدونة.', 'success');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.getBlogErrorMessage(error, 'delete'));
          this.isSaving.set(false);
          this.snackbar.show(this.errorMessage(), 'error');
        },
      });
  }

  private buildPreviewHtml(html: string): string {
    if (!html) {
      return '';
    }

    const normalized = this.normalizeLegacyTags(html);
    const sanitized = this.sanitizer.sanitize(SecurityContext.HTML, normalized);
    return sanitized ?? '';
  }

  private normalizeLegacyTags(html: string): string {
    const parser = new DOMParser();
    const doc = parser.parseFromString(html, 'text/html');
    doc.querySelectorAll('font[color]').forEach((node) => {
      const color = node.getAttribute('color');
      const span = doc.createElement('span');
      if (color) {
        span.style.color = color;
      }
      span.innerHTML = node.innerHTML;
      node.replaceWith(span);
    });

    doc.querySelectorAll('a[href]').forEach((link) => {
      const href = link.getAttribute('href') ?? '';
      if (href && !/^https?:\/\//i.test(href)) {
        link.setAttribute('href', `https://${href}`);
      }
      link.setAttribute('target', '_blank');
      link.setAttribute('rel', 'noreferrer noopener');
    });

    return doc.body.innerHTML;
  }

  private getBlogErrorMessage(
    error: unknown,
    action: 'load' | 'save' | 'delete'
  ): string {
    const actionLabel = action === 'load' ? 'تحميل' : action === 'save' ? 'حفظ' : 'حذف';

    if (!(error instanceof HttpErrorResponse)) {
      return `تعذر ${actionLabel} المدونة بسبب خطأ غير متوقع.`;
    }

    if (error.status === 0) {
      return 'تعذر الاتصال بالخادم. تأكد أن السيرفر يعمل.';
    }

    if (error.status === 401 || error.status === 403) {
      return 'ليست لديك صلاحية. سجل الدخول بحساب إداري.';
    }

    if (error.status >= 500) {
      return 'حدث خطأ في الخادم. حاول مرة أخرى لاحقاً.';
    }

    if (action === 'save' && error.status === 400) {
      return 'بيانات المدونة غير صحيحة. تحقق من الحقول المطلوبة.';
    }

    return `تعذر ${actionLabel} المدونة. حاول مرة أخرى.`;
  }
}
