import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult, TaskItemDto, TaskItemStatus, TaskPriority } from '../shared/models/models';

export interface CreateTaskPayload {
  name: string;
  description?: string;
  categoryId?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string;
  tags: string[];
}


@Injectable({ providedIn: 'root' })
export class TaskService {
  private http = inject(HttpClient);

  getTasks(page = 1, pageSize = 20, status?: TaskItemStatus, priority?: TaskPriority, search?: string, categoryId?: string): Observable<PagedResult<TaskItemDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status !== undefined) params = params.set('status', status);
    if (priority !== undefined) params = params.set('priority', priority);
    if (search) params = params.set('search', search);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get<PagedResult<TaskItemDto>>('/api/tasks', { params });
  }

  getTask(id: string): Observable<TaskItemDto> {
    return this.http.get<TaskItemDto>(`/api/tasks/${id}`);
  }

  createTask(payload: CreateTaskPayload): Observable<TaskItemDto> {
    return this.http.post<TaskItemDto>('/api/tasks', payload);
  }

  updateTask(id: string, payload: CreateTaskPayload): Observable<TaskItemDto> {
    return this.http.put<TaskItemDto>(`/api/tasks/${id}`, { id, ...payload });
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`/api/tasks/${id}`);
  }
}
