import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { AuthService } from './auth.service';
import { IUnitDetails, UnitsPage } from '../../models/IUnit';
import { UnitSearchParams } from './unit.service';

@Injectable({ providedIn: 'root' })
export class AdminUnitService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getUnits(params: UnitSearchParams = {}): Observable<UnitsPage> {
    let p = new HttpParams();
    if (params.page)       p = p.set('page', params.page);
    if (params.pageSize)   p = p.set('pageSize', params.pageSize);
    if (params.city)       p = p.set('city', params.city);
    if (params.type)       p = p.set('type', params.type);
    if (params.status)     p = p.set('status', params.status);
    if (params.rooms)      p = p.set('rooms', params.rooms);
    if (params.minPrice)   p = p.set('minPrice', params.minPrice);
    if (params.maxPrice)   p = p.set('maxPrice', params.maxPrice);
    if (params.projectId)  p = p.set('projectId', params.projectId);
    if (params.buildingId) p = p.set('buildingId', params.buildingId);

    return this.http.get<UnitsPage>(`${this.baseUrl}/api/admin/units`, {
      params: p,
      headers: this.buildAuthHeaders(),
    });
  }

  getUnit(unitId: number): Observable<IUnitDetails> {
    return this.http.get<IUnitDetails>(`${this.baseUrl}/api/admin/units/${unitId}`, {
      headers: this.buildAuthHeaders(),
    });
  }

  createUnit(payload: FormData): Observable<IUnitDetails> {
    return this.http.post<IUnitDetails>(`${this.baseUrl}/api/admin/units`, payload, {
      headers: this.buildAuthHeaders(),
    });
  }

  updateUnit(unitId: number, payload: FormData): Observable<IUnitDetails> {
    return this.http.put<IUnitDetails>(`${this.baseUrl}/api/admin/units/${unitId}`, payload, {
      headers: this.buildAuthHeaders(),
    });
  }

  deleteUnit(unitId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/admin/units/${unitId}`, {
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
