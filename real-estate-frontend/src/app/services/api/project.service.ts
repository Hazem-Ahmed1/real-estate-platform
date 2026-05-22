import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { IProject, ProjectsPage, IProjectDetails } from '../../models/IProject';

export interface ProjectSearchParams {
  page?: number;
  pageSize?: number;
  city?: string;
  status?: string;
  type?: string;
  rooms?: number;
  minPrice?: number;
  maxPrice?: number;
}

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getProjects(params: ProjectSearchParams = {}): Observable<ProjectsPage> {
    let p = new HttpParams();
    if (params.page)     p = p.set('page', params.page);
    if (params.pageSize) p = p.set('pageSize', params.pageSize);
    if (params.city)     p = p.set('city', params.city);
    if (params.status)   p = p.set('status', params.status);
    if (params.type)     p = p.set('type', params.type);
    if (params.rooms)    p = p.set('rooms', params.rooms);
    if (params.minPrice) p = p.set('minPrice', params.minPrice);
    if (params.maxPrice) p = p.set('maxPrice', params.maxPrice);

    return this.http.get<ProjectsPage>(`${this.baseUrl}/api/projects`, { params: p });
  }

  getProject(id: number): Observable<IProjectDetails> {
    return this.http.get<IProjectDetails>(`${this.baseUrl}/api/projects/${id}`);
  }
}
