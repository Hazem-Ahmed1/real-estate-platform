import { Component, OnDestroy, OnInit, inject, signal, computed } from '@angular/core';
import { Subscription } from 'rxjs';
import { ProjectService } from '../../../../services/api/project.service';
import { ISoldProject } from '../../../../models/ISoldProject';
import { HomeSectionHeaderWithFilters } from '../../../../shared/components/home-section-header-with-filters/home-section-header-with-filters';
import { SoldProjectsList } from "../../../../shared/components/sold-projects-list/sold-projects-list";

@Component({
  selector: 'app-sold-projects-section',
  imports: [HomeSectionHeaderWithFilters, SoldProjectsList],
  templateUrl: './sold-projects-section.html',
  styleUrl: './sold-projects-section.css',
})
export class SoldProjectsSection implements OnInit, OnDestroy {
  private readonly projectService = inject(ProjectService);
  private sub?: Subscription;

  // Active status filter ('all' | 'sell' | 'rent')
  readonly activeFilter = signal('all');

  // Loaded sold projects from API
  readonly allSoldProjects = signal<ISoldProject[]>([]);
  readonly hasLoaded = signal(false);

  // Computed filtered projects based on activeFilter signal
  readonly projects = computed(() => {
    const list = this.allSoldProjects();
    const filterVal = this.activeFilter();
    if (filterVal === 'all') {
      return list;
    }
    const targetType = filterVal === 'sell' ? 'تم البيع' : 'تم الإيجار';
    return list.filter(p => p.type === targetType);
  });

  readonly emptyStateMessage = computed(() => {
    const filterVal = this.activeFilter();
    if (filterVal === 'sell') {
      return 'لا توجد مشاريع مباعة حالياً';
    }
    if (filterVal === 'rent') {
      return 'لا توجد مشاريع مؤجرة حالياً';
    }
    return 'لا توجد مشاريع مباعة أو مؤجرة حالياً';
  });

  ngOnInit(): void {
    this.loadSoldProjects();
  }

  filter(val: string): void {
    this.activeFilter.set(val);
  }

  private loadSoldProjects(): void {
    this.sub?.unsubscribe();
    this.sub = this.projectService.getProjects({ pageSize: 6 }).subscribe({
      next: (page) => {
        // filter projects with status 'Sold' or 'Rented'
        const soldItems = page.items.filter(p => p.status === 'Sold' || p.status === 'Rented');
        const mapped: ISoldProject[] = soldItems.map(p => ({
          id: p.projectId,
          title: p.name,
          location: [p.city, p.region].filter(Boolean).join(' - '),
          price: 'اتصل بنا',
          imageURL: p.thumbnailUrl || 'images/p1.jpg',
          area: p.landArea ? `${p.landArea} * ${p.landArea}` : undefined,
          units: p.unitsNumber,
          buildings: p.buildingsNumber,
          rooms: p.totalRooms,
          lounges: p.totalHalls,
          type: p.status === 'Sold' ? 'تم البيع' : 'تم الإيجار'
        }));
        this.allSoldProjects.set(mapped);
        this.hasLoaded.set(true);
      },
      error: () => {
        this.allSoldProjects.set([]);
        this.hasLoaded.set(true);
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
