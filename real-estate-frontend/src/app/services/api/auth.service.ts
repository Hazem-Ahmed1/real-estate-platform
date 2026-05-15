import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { LoginRequest, LoginResponse } from '../../models/AuthModels';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_CONFIG.baseUrl;

  login(payload: LoginRequest, rememberMe: boolean): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/api/auth/login`, payload)
      .pipe(tap((response) => this.storeToken(response, rememberMe)));
  }

  logout(): void {
    if (typeof window === 'undefined') {
      return;
    }

    window.localStorage.removeItem('admin-token');
    window.localStorage.removeItem('admin-expires-at');
    window.sessionStorage.removeItem('admin-token');
    window.sessionStorage.removeItem('admin-expires-at');
  }

  getToken(): string | null {
    if (typeof window === 'undefined') {
      return null;
    }

    return window.localStorage.getItem('admin-token') ?? window.sessionStorage.getItem('admin-token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private storeToken(response: LoginResponse, rememberMe: boolean): void {
    if (typeof window === 'undefined') {
      return;
    }

    const storage = rememberMe ? window.localStorage : window.sessionStorage;
    storage.setItem('admin-token', response.token);
    storage.setItem('admin-expires-at', response.expiresAtUtc);
  }
}
