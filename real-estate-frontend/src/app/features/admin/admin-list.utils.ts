import { HttpErrorResponse } from '@angular/common/http';

/** Shared admin list page size (projects, units, buildings, features, insurances). */
export const ADMIN_LIST_PAGE_SIZE = 5;

export interface ApiPagedResponse<T> {
  items?: T[];
  totalPages?: number;
  page?: number;
  pageSize?: number;
  totalItems?: number;
}

/** Normalize API paginated payload (camelCase / PascalCase). */
export function normalizePagedResponse<T>(response: ApiPagedResponse<T> | null | undefined): {
  items: T[];
  totalPages: number;
} {
  const raw = response as Record<string, unknown> | null | undefined;
  const items = (response?.items ?? raw?.['Items']) as T[] | undefined;
  const totalPages = Number(
    response?.totalPages ?? raw?.['TotalPages'] ?? 1,
  );

  return {
    items: Array.isArray(items) ? items : [],
    totalPages: Number.isFinite(totalPages) && totalPages > 0 ? totalPages : 1,
  };
}

export function clampAdminPage(currentPage: number, totalPages: number): number {
  const max = Math.max(1, totalPages);
  return Math.min(Math.max(1, currentPage), max);
}

export function normalizeBuildingType(type: string | number | undefined | null): 'Sale' | 'Rent' {
  if (type === 2 || type === '2' || type === 'Rent') {
    return 'Rent';
  }
  return 'Sale';
}

export function translateAdminBuildingType(type: string | number | undefined | null): string {
  switch (normalizeBuildingType(type)) {
    case 'Rent':
      return 'للإيجار';
    case 'Sale':
    default:
      return 'للبيع';
  }
}

export function translateAdminStatus(status: string | number | undefined | null): string {
  switch (status) {
    case 'Sale':
      return 'للبيع';
    case 'Rent':
      return 'للإيجار';
    case 'Sold':
      return 'مباع';
    case 'Rented':
      return 'مؤجر';
    default:
      return status == null ? '-' : String(status);
  }
}

export function extractAdminApiError(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const httpError = error;
  if (httpError.status === 0) {
    return 'تعذر الاتصال بالخادم. تأكد أن الـ API يعمل.';
  }
  if (httpError.status === 401 || httpError.status === 403) {
    return 'ليست لديك صلاحية. سجّل الدخول بحساب إداري.';
  }

  const body = httpError.error;
  if (typeof body === 'string' && body.trim()) {
    return body;
  }

  if (body && typeof body === 'object') {
    const record = body as Record<string, unknown>;
    const apiMessage = record['errorMessage'] ?? record['message'] ?? record['ErrorMessage'];
    if (typeof apiMessage === 'string' && apiMessage.trim()) {
      return apiMessage;
    }

    const validationErrors = record['errors'] ?? record['Errors'];
    if (Array.isArray(validationErrors) && validationErrors.length > 0) {
      const first = validationErrors[0] as Record<string, unknown>;
      const nested = first['errors'] ?? first['Errors'] ?? first['messages'];
      if (Array.isArray(nested) && nested[0]) {
        return String(nested[0]);
      }
    }
  }

  return fallback;
}
