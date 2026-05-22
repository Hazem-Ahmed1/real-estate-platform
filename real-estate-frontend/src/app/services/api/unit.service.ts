import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { UnitCardModel, UnitsPage, IUnitDetails } from '../../models/IUnit';

export interface UnitSearchParams {
  page?: number;
  pageSize?: number;
  city?: string;
  type?: string;
  status?: string;
  rooms?: number;
  minPrice?: number;
  maxPrice?: number;
  projectId?: number;
  buildingId?: number;
}

@Injectable({ providedIn: 'root' })
export class UnitService {
  private readonly http = inject(HttpClient);
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

    return this.http.get<UnitsPage>(`${this.baseUrl}/api/units`, { params: p });
  }

  getUnit(id: number): Observable<IUnitDetails> {
    return this.http.get<IUnitDetails>(`${this.baseUrl}/api/units/${id}`);
  }
}
