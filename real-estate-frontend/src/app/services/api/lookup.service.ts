import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { AuthService } from './auth.service';
import {
  FeatureDto,
  InsuranceDto,
  LookupUpsertDto,
  UpdateFeatureDto,
  UpdateInsuranceDto,
} from '../../models/lookups.model';

export enum LookupStatus {
  Active = 0,
  Inactive = 1,
  All = 2,
}

@Injectable({
  providedIn: 'root',
})
export class LookupService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/api/admin/lookups`;
  
  private buildAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken() ?? '';
    if (!token) {
      return new HttpHeaders();
    }
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  // --- Features ---

  getFeatures(status: LookupStatus = LookupStatus.All): Observable<FeatureDto[]> {
    let params = new HttpParams();
    params = params.set('status', status.toString());
    return this.http.get<FeatureDto[]>(`${this.baseUrl}/features`, { params, headers: this.buildAuthHeaders() });
  }

  getFeatureById(id: number): Observable<FeatureDto> {
    return this.http.get<FeatureDto>(`${this.baseUrl}/features/${id}`, { headers: this.buildAuthHeaders() });
  }

  createFeature(dto: LookupUpsertDto): Observable<FeatureDto> {
    return this.http.post<FeatureDto>(`${this.baseUrl}/features`, dto, { headers: this.buildAuthHeaders() });
  }

  updateFeature(id: number, dto: UpdateFeatureDto): Observable<FeatureDto> {
    return this.http.put<FeatureDto>(`${this.baseUrl}/features/${id}`, dto, { headers: this.buildAuthHeaders() });
  }

  deleteFeature(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/features/${id}`, { headers: this.buildAuthHeaders() });
  }

  // --- Insurances (Guarantees) ---

  getInsurances(status: LookupStatus = LookupStatus.All): Observable<InsuranceDto[]> {
    let params = new HttpParams();
    params = params.set('status', status.toString());
    return this.http.get<InsuranceDto[]>(`${this.baseUrl}/insurance`, { params, headers: this.buildAuthHeaders() });
  }

  getInsuranceById(id: number): Observable<InsuranceDto> {
    return this.http.get<InsuranceDto>(`${this.baseUrl}/insurance/${id}`, { headers: this.buildAuthHeaders() });
  }

  createInsurance(dto: LookupUpsertDto): Observable<InsuranceDto> {
    return this.http.post<InsuranceDto>(`${this.baseUrl}/insurance`, dto, { headers: this.buildAuthHeaders() });
  }

  updateInsurance(id: number, dto: UpdateInsuranceDto): Observable<InsuranceDto> {
    return this.http.put<InsuranceDto>(`${this.baseUrl}/insurance/${id}`, dto, { headers: this.buildAuthHeaders() });
  }

  deleteInsurance(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/insurance/${id}`, { headers: this.buildAuthHeaders() });
  }
}
