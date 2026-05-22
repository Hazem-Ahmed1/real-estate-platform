import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';

export interface SearchFilterOptions {
  cities: string[];
}

@Injectable({ providedIn: 'root' })
export class SearchOptionsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getSearchOptions(): Observable<SearchFilterOptions> {
    return this.http.get<SearchFilterOptions>(`${this.baseUrl}/api/filters/search-options`);
  }
}
