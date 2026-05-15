import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../core/config/api.config';
import { AdminBlogDto } from '../../models/AdminBlogDto';
import { PublicBlogDto } from '../../models/PublicBlogDto';
import { PaginatedResponse } from '../../models/PaginatedResponse';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class BlogService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = API_CONFIG.baseUrl;

  getBlogs(params?: { page?: number; pageSize?: number; search?: string; sort?: string; q?: string }):
    Observable<PaginatedResponse<PublicBlogDto>> {
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

    return this.http.get<PaginatedResponse<PublicBlogDto>>(`${this.baseUrl}/api/blogs`, {
      params: httpParams,
    });
  }

  getBlog(blogId: number): Observable<PublicBlogDto> {
    return this.http.get<PublicBlogDto>(`${this.baseUrl}/api/blogs/${blogId}`);
  }

  createBlog(payload: FormData): Observable<AdminBlogDto> {
    return this.http.post<AdminBlogDto>(`${this.baseUrl}/api/blogs`, payload, {
      headers: this.buildAuthHeaders(),
    });
  }

  updateBlog(blogId: number, payload: FormData): Observable<AdminBlogDto> {
    return this.http.put<AdminBlogDto>(`${this.baseUrl}/api/blogs/${blogId}`, payload, {
      headers: this.buildAuthHeaders(),
    });
  }

  deleteBlog(blogId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/blogs/${blogId}`, {
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
