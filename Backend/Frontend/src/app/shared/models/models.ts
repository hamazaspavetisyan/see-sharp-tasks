export interface LoginResponse {
  token: string;
  expiresAt: string;
}

export interface RegisterResponse {
  id: string;
  email: string;
}

export interface UserDto {
  id: string;
  email: string;
  createdAt: string;
}

export enum TaskItemStatus {
  Todo = 0,
  InProgress = 1,
  Done = 2,
  Cancelled = 3,
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export interface TaskItemDto {
  id: string;
  name: string;
  description?: string;
  categoryId?: string;
  categoryName?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string;
  tags: string[];
  userId: string;
  createdAt: string;
  updatedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CategoryDto {
  id: string;
  name: string;
  userId: string;
  createdAt: string;
}
