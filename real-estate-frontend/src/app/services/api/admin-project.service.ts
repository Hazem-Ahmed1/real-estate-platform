import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { AuthService } from './auth.service';
import { IProjectDetails, ProjectsPage } from '../../models/IProject';
import { ProjectSearchParams } from './project.service';

@Injectable({ providedIn: 'root' })
export class AdminProjectService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getProjects(params: ProjectSearchParams = {}): Observable<ProjectsPage> {
    let p = new HttpParams();
    if (params.page) p = p.set('page', params.page);
    if (params.pageSize) p = p.set('pageSize', params.pageSize);
    if (params.city) p = p.set('city', params.city);
    if (params.status) p = p.set('status', params.status);
    if (params.type) p = p.set('type', params.type);
    if (params.rooms) p = p.set('rooms', params.rooms);
    if (params.minPrice) p = p.set('minPrice', params.minPrice);
    if (params.maxPrice) p = p.set('maxPrice', params.maxPrice);

    return this.http.get<ProjectsPage>(`${this.baseUrl}/api/admin/projects`, {
      params: p,
      headers: this.buildAuthHeaders(),
    });
  }

  getProject(projectId: number): Observable<IProjectDetails> {
    return this.http.get<IProjectDetails>(`${this.baseUrl}/api/admin/projects/${projectId}`, {
      headers: this.buildAuthHeaders(),
    });
  }

  createProject(payload: FormData): Observable<IProjectDetails> {
    return this.http.post<IProjectDetails>(`${this.baseUrl}/api/admin/projects`, payload, {
      headers: this.buildAuthHeaders(),
    });
  }

  updateProject(projectId: number, payload: FormData): Observable<IProjectDetails> {
    return this.http.put<IProjectDetails>(`${this.baseUrl}/api/admin/projects/${projectId}`, payload, {
      headers: this.buildAuthHeaders(),
    });
  }

  deleteProject(projectId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/admin/projects/${projectId}`, {
      headers: this.buildAuthHeaders(),
    });
  }

  private buildAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken() ?? '';
    if (!token) {
      return new HttpHeaders();
    }

    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }
}
