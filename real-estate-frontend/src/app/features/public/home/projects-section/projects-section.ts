import { Component, OnDestroy, OnInit, inject, signal, computed } from '@angular/core';
import { Subscription } from 'rxjs';
import { IProject } from '../../../../models/IProject';
import { ProjectService } from '../../../../services/api/project.service';
import { HomeSectionHeaderWithFilters } from '../../../../shared/components/home-section-header-with-filters/home-section-header-with-filters';
import { ProjectsList } from '../../../../shared/components/projects-list/projects-list';

@Component({
  selector: 'app-projects-section',
  imports: [HomeSectionHeaderWithFilters, ProjectsList],
  templateUrl: './projects-section.html',
  styleUrl: './projects-section.css',
})
export class ProjectsSection implements OnInit, OnDestroy {
  private readonly projectService = inject(ProjectService);
  private sub?: Subscription;
  private readonly allowedStatuses = ['Sale', 'Rent'];

  // Signal — live data from GET /api/projects?pageSize=6
  readonly allProjects = signal<IProject[]>([]);
  readonly hasLoaded = signal(false);

  // Active status filter ('all' | 'sell' | 'rent')
  readonly activeFilter = signal('all');

  // Filtered projects signal (computed dynamically from allProjects and activeFilter)
  readonly projects = computed(() => {
    const list = this.allProjects().filter(p => this.allowedStatuses.includes(p.status));
    const filterVal = this.activeFilter();
    if (filterVal === 'all') {
      return list;
    }
    const status = filterVal === 'sell' ? this.allowedStatuses[0] : this.allowedStatuses[1];
    return list.filter(p => p.status === status);
  });

  readonly emptyStateMessage = computed(() => {
    const filterVal = this.activeFilter();
    if (filterVal === 'sell') {
      return 'لا توجد مشاريع معروضة للبيع حالياً';
    }
    if (filterVal === 'rent') {
      return 'لا توجد مشاريع معروضة للإيجار حالياً';
    }
    return 'لا توجد مشاريع متاحة حالياً';
  });

  ngOnInit(): void {
    this.loadProjects();
  }

  filter(val: string): void {
    this.activeFilter.set(val);
  }

  private loadProjects(): void {
    this.sub?.unsubscribe();
    this.sub = this.projectService
      .getProjects({ pageSize: 6 })
      .subscribe({
        next: (page) => {
          this.allProjects.set(page.items ?? []);
          this.hasLoaded.set(true);
        },
        error: () => {
          this.allProjects.set([]);
          this.hasLoaded.set(true);
        },
      });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
