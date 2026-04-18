import { Component, input } from '@angular/core';
import { RouterModule } from '@angular/router';

export interface BreadcrumbItem {
  label: string;
  url?: string;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './breadcrumbs.html',
  styleUrl: './breadcrumbs.css',
})
export class BreadcrumbComponent {
  items = input.required<BreadcrumbItem[]>();
  price = input<number | null>(null);
  status = input<string | null>(null);
}
