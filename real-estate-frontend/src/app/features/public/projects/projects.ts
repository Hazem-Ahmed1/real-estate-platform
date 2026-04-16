import { Component } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";

@Component({
  selector: 'app-projects',
  imports: [BreadcrumbComponent],
  templateUrl: './projects.html',
  styleUrl: './projects.css',
})
export class Projects {}
