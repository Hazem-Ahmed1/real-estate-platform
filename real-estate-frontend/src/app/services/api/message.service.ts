import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { AdminMessageDetailsDto, AdminMessageListDto } from '../../models/AdminMessageDto';
import { PublicMessageCreateDto, PublicMessageCreatedDto } from '../../models/PublicMessageDto';
import { PaginatedResponse } from '../../models/PaginatedResponse';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class MessageService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getMessages(params?: { page?: number; pageSize?: number; search?: string; sort?: string; q?: string }):
    Observable<PaginatedResponse<AdminMessageListDto>> {
    let httpParams = new HttpParams();

    if (params?.page) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }
    if (params?.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params?.sort) {
      httpParams = httpParams.set('sort', params.sort);
    }
    if (params?.q) {
      httpParams = httpParams.set('q', params.q);
    }

    return this.http.get<PaginatedResponse<AdminMessageListDto>>(`${this.baseUrl}/api/admin/messages`, {
      params: httpParams,
      headers: this.buildAuthHeaders(),
    });
  }

  getMessage(messageId: number): Observable<AdminMessageDetailsDto> {
    return this.http.get<AdminMessageDetailsDto>(`${this.baseUrl}/api/admin/messages/${messageId}`, {
      headers: this.buildAuthHeaders(),
    });
  }

  deleteMessage(messageId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/admin/messages/${messageId}`, {
      headers: this.buildAuthHeaders(),
    });
  }

  createMessage(payload: PublicMessageCreateDto): Observable<PublicMessageCreatedDto> {
    return this.http.post<PublicMessageCreatedDto>(`${this.baseUrl}/api/messages`, payload);
  }

  private buildAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken() ?? '';
    if (!token) {
      return new HttpHeaders();
    }

    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }
}
