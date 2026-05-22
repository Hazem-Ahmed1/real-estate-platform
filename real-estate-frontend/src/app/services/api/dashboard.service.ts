import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { AuthService } from './auth.service';

export interface PublicStats {
  totalProjects: number;
  totalBuildings: number;
  totalUnits: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getPublicStats(): Observable<PublicStats> {
    return this.http.get<PublicStats>(`${this.baseUrl}/api/dashboard/stats`);
  }

  getAdminStats(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/api/admin/dashboard/stats`, {
      headers: this.buildAuthHeaders(),
    });
  }

  getAdminChart(filter: 'thisYear' | 'prevYear' | 'all' = 'thisYear'): Observable<any> {
    const params = { filter };
    return this.http.get<any>(`${this.baseUrl}/api/admin/dashboard/chart`, {
      headers: this.buildAuthHeaders(),
      params,
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
