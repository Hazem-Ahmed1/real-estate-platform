import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { AuthService } from './auth.service';
import { AdminBuildingDto, AdminBuildingUpsertDto } from '../../models/AdminBuildingDto';

@Injectable({ providedIn: 'root' })
export class AdminBuildingService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getBuildings(projectId?: number): Observable<AdminBuildingDto[]> {
    let params = new HttpParams();
    if (projectId) {
      params = params.set('projectId', projectId);
    }

    return this.http.get<AdminBuildingDto[]>(`${this.baseUrl}/api/admin/buildings`, {
      params,
      headers: this.buildAuthHeaders(),
    });
  }

  getBuilding(buildingId: number): Observable<AdminBuildingDto> {
    return this.http.get<AdminBuildingDto>(`${this.baseUrl}/api/admin/buildings/${buildingId}`, {
      headers: this.buildAuthHeaders(),
    });
  }

  createBuilding(payload: AdminBuildingUpsertDto): Observable<AdminBuildingDto> {
    return this.http.post<AdminBuildingDto>(`${this.baseUrl}/api/admin/buildings`, payload, {
      headers: this.buildJsonAuthHeaders(),
    });
  }

  updateBuilding(buildingId: number, payload: AdminBuildingUpsertDto): Observable<AdminBuildingDto> {
    return this.http.put<AdminBuildingDto>(`${this.baseUrl}/api/admin/buildings/${buildingId}`, payload, {
      headers: this.buildJsonAuthHeaders(),
    });
  }

  deleteBuilding(buildingId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/admin/buildings/${buildingId}`, {
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

  private buildJsonAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken() ?? '';
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    return new HttpHeaders(headers);
  }
}
