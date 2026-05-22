import { Injectable, inject, signal } from '@angular/core';
import { IProject, ProjectsPage } from '../models/IProject';
import { UnitCardModel, UnitsPage } from '../models/IUnit';
import { ProjectService } from './api/project.service';
import { UnitService } from './api/unit.service';

@Injectable({ providedIn: 'root' })
export class SearchStateService {
  private readonly projectService = inject(ProjectService);
  private readonly unitService = inject(UnitService);
  private readonly allowedStatuses = new Set(['Sale', 'Rent']);

  readonly hasSearched = signal(false);
  readonly searchType = signal<'project' | 'unit'>('project');
  readonly projectsResults = signal<IProject[]>([]);
  readonly unitsResults = signal<UnitCardModel[]>([]);
  readonly isLoading = signal(false);
  readonly currentPage = signal(1);
  readonly totalPages = signal(1);
  readonly pageSize = signal(6);
  readonly lastParams = signal<Record<string, any>>({});

  reset(): void {
    this.hasSearched.set(false);
    this.projectsResults.set([]);
    this.unitsResults.set([]);
    this.isLoading.set(false);
    this.currentPage.set(1);
    this.totalPages.set(1);
    this.lastParams.set({});
  }

  search(params: Record<string, any>, type: 'project' | 'unit', page = 1): void {
    const requestParams = { page, pageSize: this.pageSize(), ...params };

    this.searchType.set(type);
    this.hasSearched.set(true);
    this.isLoading.set(true);
    this.currentPage.set(page);
    this.lastParams.set(params);

    if (type === 'project') {
      this.projectService.getProjects(requestParams).subscribe({
        next: (pageData: ProjectsPage) => {
          const items = (pageData.items ?? []).filter(p => this.allowedStatuses.has(p.status));
          this.projectsResults.set(items);
          this.totalPages.set(pageData.totalPages ?? 1);
          this.isLoading.set(false);
        },
        error: () => {
          this.projectsResults.set([]);
          this.totalPages.set(1);
          this.isLoading.set(false);
        }
      });
    } else {
      this.unitService.getUnits(requestParams).subscribe({
        next: (pageData: UnitsPage) => {
          const items = (pageData.items ?? []).filter(u => this.allowedStatuses.has(u.status));
          this.unitsResults.set(items);
          this.totalPages.set(pageData.totalPages ?? 1);
          this.isLoading.set(false);
        },
        error: () => {
          this.unitsResults.set([]);
          this.totalPages.set(1);
          this.isLoading.set(false);
        }
      });
    }
  }
}
