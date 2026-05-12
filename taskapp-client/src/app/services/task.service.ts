import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { PagedTasksResponse, TaskItem, TaskUpsertRequest } from '../models/task.model';

export interface TaskQuery {
  search?: string;
  status?: string;
  priority?: string;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly baseUrl = `${environment.apiUrl}/tasks`;

  constructor(private readonly http: HttpClient) {}

  getTasks(query: TaskQuery): Observable<PagedTasksResponse> {
    const page = query.page ?? 1;
    const pageSize = query.pageSize ?? 10;
    let params = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));

    const optional: (keyof TaskQuery)[] = ['search', 'status', 'priority', 'sortBy', 'sortOrder'];
    for (const key of optional) {
      const value = query[key];
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }

    return this.http
      .get<unknown>(this.baseUrl, { params, withCredentials: true })
      .pipe(map((body) => TaskService.normalizePagedTasks(body)));
  }

  create(payload: TaskUpsertRequest): Observable<TaskItem> {
    return this.http
      .post<unknown>(this.baseUrl, payload, { withCredentials: true })
      .pipe(map((body) => TaskService.normalizeTaskItem(body)));
  }

  update(id: number, payload: TaskUpsertRequest): Observable<TaskItem> {
    return this.http
      .put<unknown>(`${this.baseUrl}/${id}`, payload, { withCredentials: true })
      .pipe(map((body) => TaskService.normalizeTaskItem(body)));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }

  markCompleted(id: number): Observable<TaskItem> {
    return this.http
      .patch<unknown>(`${this.baseUrl}/${id}/complete`, {}, { withCredentials: true })
      .pipe(map((body) => TaskService.normalizeTaskItem(body)));
  }

  /** Handles camelCase or PascalCase from JSON (e.g. `items` vs `Items`, `id` vs `Id`). */
  private static normalizePagedTasks(body: unknown): PagedTasksResponse {
    if (!body || typeof body !== 'object') {
      return { items: [], totalCount: 0, page: 1, pageSize: 10 };
    }
    const o = body as Record<string, unknown>;
    const rawItems = o['items'] ?? o['Items'];
    const items = Array.isArray(rawItems)
      ? rawItems.map((row) => TaskService.normalizeTaskItem(row))
      : [];
    return {
      items,
      totalCount: Number(o['totalCount'] ?? o['TotalCount'] ?? 0),
      page: Number(o['page'] ?? o['Page'] ?? 1),
      pageSize: Number(o['pageSize'] ?? o['PageSize'] ?? 10)
    };
  }

  private static normalizeTaskItem(body: unknown): TaskItem {
    if (!body || typeof body !== 'object') {
      return {
        id: 0,
        title: '',
        isCompleted: false,
        priority: 'Medium',
        createdAt: '',
        updatedAt: ''
      };
    }
    const t = body as Record<string, unknown>;
    return {
      id: Number(t['id'] ?? t['Id'] ?? 0),
      title: String(t['title'] ?? t['Title'] ?? ''),
      description: (t['description'] ?? t['Description']) as string | undefined,
      isCompleted: Boolean(t['isCompleted'] ?? t['IsCompleted']),
      priority: (t['priority'] ?? t['Priority'] ?? 'Medium') as TaskItem['priority'],
      dueDate: (t['dueDate'] ?? t['DueDate']) as string | null | undefined,
      createdAt: String(t['createdAt'] ?? t['CreatedAt'] ?? ''),
      updatedAt: String(t['updatedAt'] ?? t['UpdatedAt'] ?? '')
    };
  }
}
