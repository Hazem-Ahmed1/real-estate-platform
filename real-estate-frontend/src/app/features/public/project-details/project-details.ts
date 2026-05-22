import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { IProject, IProjectDetails } from '../../../models/IProject';
import { ProjectService } from '../../../services/api/project.service';
import { SectionTitle } from '../../../shared/components/section-title/section-title';
import { ProjectGallery } from '../../../shared/components/project-gallery/project-gallery';
import { IconFeatureGrid } from '../../../shared/components/icon-feature-grid/icon-feature-grid';
import { ProjectMap } from '../../../shared/components/project-map/project-map';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs';
import { ProjectHero } from './project-hero/project-hero';
import { ProjectPanoramaSection } from './project-panorama-section/project-panorama-section';
import { ProjectVideoSection } from './project-video-section/project-video-section';

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [
    ContactSection,
    SectionTitle,
    ProjectGallery,
    IconFeatureGrid,
    ProjectMap,
    BreadcrumbComponent,
    ProjectHero,
    ProjectPanoramaSection,
    ProjectVideoSection,
  ],
  templateUrl: './project-details.html',
  styleUrl: './project-details.css',
})
export class ProjectDetails implements OnInit, OnDestroy {
  private readonly projectService = inject(ProjectService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private sub?: Subscription;

  readonly project = signal<IProjectDetails | null>(null);

  computeCircleRadiusMeters(proj: IProjectDetails | null): number | null {
    if (!proj) return null;
    // prefer buildUpArea, fall back to totalBuildingArea when available
    const area = proj.buildUpArea ?? (proj.totalBuildingArea ?? null);
    if (area == null || area <= 0) return null;
    return Math.max(150, Math.min(2200, Math.sqrt(area / Math.PI)));
  }

  get stats() {
    const proj = this.project();
    if (!proj) return [];
    return [
      { label: 'عدد الوحدات السكنية', value: proj.unitsNumber, iconUrl: '/images/img_Units_builds.jpg' },
      { label: 'عدد المباني', value: proj.buildingsNumber, iconUrl: '/images/img_Units_builds.jpg' },
    ];
  }

  get resolvedFeatures() {
    const proj = this.project();
    if (!proj?.features) return [];
    return proj.features.map(f => ({
      label: f.name,
      iconClass: 'fa-circle-check',
    }));
  }

  get resolvedWarranties() {
    const proj = this.project();
    if (!proj?.insurance) return [];
    return proj.insurance.map(i => ({
      label: i.name,
      iconClass: 'fa-shield-halved',
      subLabel: this.formatWarrantyDuration(i.duration),
    }));
  }

  private formatWarrantyDuration(duration?: number): string {
    const value = duration ?? 0;
    return `+ ${value.toString().padStart(2, '0')} سنة`;
  }

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe(params => {
      const rawId = params.get('id');
      const id = rawId ? Number(rawId) : NaN;
      if (!Number.isFinite(id)) {
        this.router.navigate(['/']);
        return;
      }

      this.project.set(null);

      this.projectService.getProject(id).subscribe({
        next: (project) => {
          if (!project) {
            this.router.navigate(['/']);
            return;
          }
          this.project.set(project);
        },
        error: () => {
          this.router.navigate(['/']);
        }
      });
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
