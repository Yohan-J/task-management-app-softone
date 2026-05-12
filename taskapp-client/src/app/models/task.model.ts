export interface TaskItem {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  priority: 'Low' | 'Medium' | 'High';
  dueDate?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TaskUpsertRequest {
  title: string;
  description?: string;
  isCompleted: boolean;
  priority: 'Low' | 'Medium' | 'High';
  dueDate?: string | null;
}

export interface PagedTasksResponse {
  items: TaskItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}
