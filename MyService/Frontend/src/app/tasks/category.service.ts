import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoryDto } from '../shared/models/models';

export interface CreateCategoryPayload {
  name: string;
}

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private http = inject(HttpClient);

  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>('/api/categories');
  }

  createCategory(payload: CreateCategoryPayload): Observable<CategoryDto> {
    return this.http.post<CategoryDto>('/api/categories', payload);
  }

  updateCategory(id: string, payload: CreateCategoryPayload): Observable<CategoryDto> {
    return this.http.put<CategoryDto>(`/api/categories/${id}`, { id, ...payload });
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`/api/categories/${id}`);
  }
}
