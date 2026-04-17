import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
})
export class PaginationComponent {
  currentPage = input<number>(1);
  totalPages = input<number>(1);

  pageChange = output<number>();

  selectPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;
    this.pageChange.emit(page);
  }

  next() {
    this.selectPage(this.currentPage() + 1);
  }

  prev() {
    this.selectPage(this.currentPage() - 1);
  }

  pages(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const maxVisible = 5;

    let start = Math.max(current - Math.floor(maxVisible / 2), 1);
    let end = start + maxVisible - 1;

    if (end > total) {
      end = total;
      start = Math.max(end - maxVisible + 1, 1);
    }

    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }
}
