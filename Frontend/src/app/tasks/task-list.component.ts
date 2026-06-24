import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { NzBadgeModule } from 'ng-zorro-antd/badge';
import { NzDrawerModule } from 'ng-zorro-antd/drawer';
import { TaskService, CreateTaskPayload } from './task.service';
import { CategoryService } from './category.service';
import { TaskItemDto, TaskItemStatus, TaskPriority, CategoryDto } from '../shared/models/models';
import { AuthService } from '../auth/auth.service';
import { UserDto } from '../shared/models/models';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    NzTableModule, NzButtonModule, NzTagModule,
    NzSelectModule, NzInputModule, NzPopconfirmModule,
    NzIconModule, NzDatePickerModule, NzDividerModule,
    NzCardModule, NzToolTipModule, NzBadgeModule, NzDrawerModule
  ],
  template: `
    <div style="display:flex;height:100vh;overflow:hidden">

      <!-- Sidebar -->
      <div style="width:220px;min-width:220px;background:#001529;color:#fff;display:flex;flex-direction:column;padding:0">
        <div style="padding:20px 16px 12px;font-size:18px;font-weight:600;color:#fff;letter-spacing:0.5px">
          Tasks App
        </div>
        <div style="padding:0 8px 8px">
          <div (click)="activeView='tasks'"
               [style.background]="activeView==='tasks' ? '#1890ff' : 'transparent'"
               style="padding:10px 16px;border-radius:6px;cursor:pointer;color:#fff;font-size:14px;margin-bottom:4px;display:flex;align-items:center;gap:8px">
            <span nz-icon nzType="check-square"></span> My Tasks
          </div>
          <div (click)="activeView='categories'"
               [style.background]="activeView==='categories' ? '#1890ff' : 'transparent'"
               style="padding:10px 16px;border-radius:6px;cursor:pointer;color:#fff;font-size:14px;display:flex;align-items:center;gap:8px">
            <span nz-icon nzType="tags"></span> Categories
          </div>
        </div>
        <div style="flex:1"></div>
        <div style="padding:12px 16px;border-top:1px solid #ffffff1a">
          <div style="font-size:12px;color:#ffffff88;margin-bottom:6px">{{ currentUser?.email }}</div>
          <button nz-button nzSize="small" (click)="logout()" style="width:100%;background:#ffffff15;border-color:#ffffff30;color:#fff">
            Logout
          </button>
        </div>
      </div>

      <!-- Main content -->
      <div style="flex:1;overflow:auto;background:#f0f2f5">

        <!-- Tasks view -->
        <div *ngIf="activeView==='tasks'" style="padding:24px">
          <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:20px">
            <h2 style="margin:0;font-size:22px">My Tasks</h2>
            <button nz-button nzType="primary" (click)="openNewTaskDrawer()">
              <span nz-icon nzType="plus"></span> New Task
            </button>
          </div>

          <!-- Filters -->
          <nz-card [nzBordered]="false" style="margin-bottom:16px;box-shadow:0 1px 4px #0000000f">
            <div style="display:flex;gap:12px;flex-wrap:wrap;align-items:center">
              <nz-select [(ngModel)]="filterStatus" (ngModelChange)="load()" nzPlaceHolder="All Statuses" nzAllowClear style="width:150px">
                <nz-option [nzValue]="0" nzLabel="Todo"></nz-option>
                <nz-option [nzValue]="1" nzLabel="In Progress"></nz-option>
                <nz-option [nzValue]="2" nzLabel="Done"></nz-option>
                <nz-option [nzValue]="3" nzLabel="Cancelled"></nz-option>
              </nz-select>
              <nz-select [(ngModel)]="filterPriority" (ngModelChange)="load()" nzPlaceHolder="All Priorities" nzAllowClear style="width:150px">
                <nz-option [nzValue]="0" nzLabel="Low"></nz-option>
                <nz-option [nzValue]="1" nzLabel="Medium"></nz-option>
                <nz-option [nzValue]="2" nzLabel="High"></nz-option>
                <nz-option [nzValue]="3" nzLabel="Critical"></nz-option>
              </nz-select>
              <nz-select [(ngModel)]="filterCategoryId" (ngModelChange)="load()" nzPlaceHolder="All Categories" nzAllowClear style="width:160px">
                <nz-option *ngFor="let c of categories" [nzValue]="c.id" [nzLabel]="c.name"></nz-option>
              </nz-select>
              <nz-input-group [nzSuffix]="searchSuffix" style="width:220px">
                <input nz-input [(ngModel)]="search" (keydown.enter)="load()" placeholder="Search tasks..." />
              </nz-input-group>
              <ng-template #searchSuffix>
                <span nz-icon nzType="search" style="cursor:pointer" (click)="load()"></span>
              </ng-template>
              <button nz-button nzType="default" (click)="clearFilters()">Clear</button>
            </div>
          </nz-card>

          <!-- Tasks table -->
          <nz-card [nzBordered]="false" style="box-shadow:0 1px 4px #0000000f">
            <nz-table #taskTable [nzData]="tasks" [nzLoading]="loading"
                      [nzTotal]="total" [nzPageIndex]="page" [nzPageSize]="pageSize"
                      (nzPageIndexChange)="onPageChange($event)" nzShowPagination
                      [nzFrontPagination]="false">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Category</th>
                  <th>Status</th>
                  <th>Priority</th>
                  <th>Due Date</th>
                  <th>Tags</th>
                  <th style="width:130px">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let t of taskTable.data">
                  <td>
                    <div style="font-weight:500">{{ t.name }}</div>
                    <div *ngIf="t.description" style="font-size:12px;color:#8c8c8c;margin-top:2px">{{ t.description }}</div>
                  </td>
                  <td>
                    <nz-tag *ngIf="t.categoryName" nzColor="geekblue">{{ t.categoryName }}</nz-tag>
                    <span *ngIf="!t.categoryName" style="color:#bfbfbf">—</span>
                  </td>
                  <td>
                    <nz-tag [nzColor]="statusColor(t.status)">{{ statusLabel(t.status) }}</nz-tag>
                  </td>
                  <td>
                    <nz-tag [nzColor]="priorityColor(t.priority)">{{ priorityLabel(t.priority) }}</nz-tag>
                  </td>
                  <td>
                    <span *ngIf="t.dueDate" [style.color]="isPastDue(t.dueDate) ? '#ff4d4f' : '#595959'">
                      {{ t.dueDate | date:'MMM d, y' }}
                    </span>
                    <span *ngIf="!t.dueDate" style="color:#bfbfbf">—</span>
                  </td>
                  <td>
                    <nz-tag *ngFor="let tag of t.tags" style="margin-bottom:2px">{{ tag }}</nz-tag>
                  </td>
                  <td>
                    <button nz-button nzSize="small" nzType="link" (click)="openEditTaskDrawer(t)" nz-tooltip nzTooltipTitle="Edit">
                      <span nz-icon nzType="edit"></span>
                    </button>
                    <button nz-button nzSize="small" nzType="link" nzDanger
                            nz-popconfirm nzPopconfirmTitle="Delete this task?"
                            (nzOnConfirm)="deleteTask(t.id)"
                            nz-tooltip nzTooltipTitle="Delete">
                      <span nz-icon nzType="delete"></span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </nz-table>
          </nz-card>
        </div>

        <!-- Categories view -->
        <div *ngIf="activeView==='categories'" style="padding:24px">
          <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:20px">
            <h2 style="margin:0;font-size:22px">Categories</h2>
            <button nz-button nzType="primary" (click)="openCategoryForm()">
              <span nz-icon nzType="plus"></span> New Category
            </button>
          </div>

          <nz-card *ngIf="showCategoryForm" [nzBordered]="false" style="margin-bottom:16px;box-shadow:0 1px 4px #0000000f">
            <div style="display:flex;gap:8px;align-items:center">
              <input nz-input [(ngModel)]="categoryForm.name" placeholder="Category name" style="max-width:300px" (keydown.enter)="saveCategory()" />
              <button nz-button nzType="primary" (click)="saveCategory()" [disabled]="!categoryForm.name.trim()">Save</button>
              <button nz-button (click)="cancelCategoryForm()">Cancel</button>
            </div>
          </nz-card>

          <nz-card [nzBordered]="false" style="box-shadow:0 1px 4px #0000000f">
            <nz-table [nzData]="categories" [nzLoading]="catLoading" [nzShowPagination]="false" nzSize="middle">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Created</th>
                  <th style="width:130px">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let c of categories">
                  <td>
                    <span *ngIf="editingCategoryId !== c.id">{{ c.name }}</span>
                    <input *ngIf="editingCategoryId === c.id" nz-input [(ngModel)]="categoryEditName"
                           style="max-width:240px" (keydown.enter)="saveInlineCategoryEdit(c.id)" />
                  </td>
                  <td>{{ c.createdAt | date:'MMM d, y' }}</td>
                  <td>
                    <span *ngIf="editingCategoryId !== c.id">
                      <button nz-button nzSize="small" nzType="link" (click)="startCategoryEdit(c)">
                        <span nz-icon nzType="edit"></span>
                      </button>
                      <button nz-button nzSize="small" nzType="link" nzDanger
                              nz-popconfirm nzPopconfirmTitle="Delete this category?"
                              (nzOnConfirm)="deleteCategory(c.id)">
                        <span nz-icon nzType="delete"></span>
                      </button>
                    </span>
                    <span *ngIf="editingCategoryId === c.id">
                      <button nz-button nzSize="small" nzType="primary" (click)="saveInlineCategoryEdit(c.id)">Save</button>
                      <button nz-button nzSize="small" style="margin-left:4px" (click)="editingCategoryId=null">Cancel</button>
                    </span>
                  </td>
                </tr>
                <tr *ngIf="categories.length === 0">
                  <td colspan="3" style="text-align:center;color:#8c8c8c;padding:32px">
                    No categories yet. Create one to organise your tasks.
                  </td>
                </tr>
              </tbody>
            </nz-table>
          </nz-card>
        </div>
      </div>
    </div>

    <!-- Task drawer (create / edit) -->
    <nz-drawer [nzVisible]="drawerVisible" [nzWidth]="680"
               [nzTitle]="editingTask ? 'Edit Task' : 'New Task'"
               (nzOnClose)="closeDrawer()">
      <ng-container *nzDrawerContent>
        <div style="display:flex;flex-direction:column;gap:14px">

          <div>
            <div style="font-size:13px;font-weight:500;margin-bottom:4px">Name <span style="color:#ff4d4f">*</span></div>
            <input nz-input [(ngModel)]="taskForm.name" placeholder="Task name" />
          </div>

          <div>
            <div style="font-size:13px;font-weight:500;margin-bottom:4px">Description</div>
            <textarea nz-input [(ngModel)]="taskForm.description" placeholder="Optional description"
                      [nzAutosize]="{ minRows: 2, maxRows: 4 }"></textarea>
          </div>

          <div style="display:flex;gap:12px">
            <div style="flex:1">
              <div style="font-size:13px;font-weight:500;margin-bottom:4px">Status</div>
              <nz-select [(ngModel)]="taskForm.status" style="width:100%">
                <nz-option [nzValue]="0" nzLabel="Todo"></nz-option>
                <nz-option [nzValue]="1" nzLabel="In Progress"></nz-option>
                <nz-option [nzValue]="2" nzLabel="Done"></nz-option>
                <nz-option [nzValue]="3" nzLabel="Cancelled"></nz-option>
              </nz-select>
            </div>
            <div style="flex:1">
              <div style="font-size:13px;font-weight:500;margin-bottom:4px">Priority</div>
              <nz-select [(ngModel)]="taskForm.priority" style="width:100%">
                <nz-option [nzValue]="0" nzLabel="Low"></nz-option>
                <nz-option [nzValue]="1" nzLabel="Medium"></nz-option>
                <nz-option [nzValue]="2" nzLabel="High"></nz-option>
                <nz-option [nzValue]="3" nzLabel="Critical"></nz-option>
              </nz-select>
            </div>
          </div>

          <div>
            <div style="font-size:13px;font-weight:500;margin-bottom:4px">Category</div>
            <nz-select [(ngModel)]="taskForm.categoryId" nzPlaceHolder="No category" nzAllowClear style="width:100%">
              <nz-option *ngFor="let c of categories" [nzValue]="c.id" [nzLabel]="c.name"></nz-option>
            </nz-select>
          </div>

          <div>
            <div style="font-size:13px;font-weight:500;margin-bottom:4px">Due Date</div>
            <nz-date-picker [(ngModel)]="taskForm.dueDate" nzPlaceHolder="No due date" [nzAllowClear]="true" style="width:100%"></nz-date-picker>
          </div>

          <div>
            <div style="font-size:13px;font-weight:500;margin-bottom:4px">Tags</div>
            <nz-select [(ngModel)]="taskForm.tags" nzMode="tags" nzPlaceHolder="Type a tag and press Enter" style="width:100%"></nz-select>
          </div>

          <nz-divider style="margin:4px 0"></nz-divider>

          <div style="display:flex;gap:8px;justify-content:flex-end">
            <button nz-button (click)="closeDrawer()">Cancel</button>
            <button nz-button nzType="primary" (click)="saveTask()" [disabled]="!taskForm.name.trim()">
              {{ editingTask ? 'Update' : 'Create' }}
            </button>
          </div>
        </div>
      </ng-container>
    </nz-drawer>
  `
})
export class TaskListComponent implements OnInit {
  tasks: TaskItemDto[] = [];
  categories: CategoryDto[] = [];
  currentUser: UserDto | null = null;

  loading = false;
  catLoading = false;
  total = 0;
  page = 1;
  pageSize = 20;

  filterStatus?: number;
  filterPriority?: number;
  filterCategoryId?: string;
  search = '';

  activeView: 'tasks' | 'categories' = 'tasks';

  // Task drawer
  drawerVisible = false;
  editingTask?: TaskItemDto;
  taskForm = this.emptyTaskForm();

  // Category form (inline new)
  showCategoryForm = false;
  categoryForm = { name: '' };

  // Category inline edit
  editingCategoryId: string | null = null;
  categoryEditName = '';

  private taskService = inject(TaskService);
  private categoryService = inject(CategoryService);
  private authService = inject(AuthService);
  private msg = inject(NzMessageService);

  ngOnInit() {
    this.loadCategories();
    this.load();
    this.authService.getMe().subscribe({ next: u => this.currentUser = u });
  }

  load() {
    this.loading = true;
    this.taskService
      .getTasks(this.page, this.pageSize, this.filterStatus as TaskItemStatus | undefined,
                this.filterPriority as TaskPriority | undefined,
                this.search || undefined, this.filterCategoryId || undefined)
      .subscribe({
        next: res => { this.tasks = res.items; this.total = res.totalCount; this.loading = false; },
        error: () => this.loading = false
      });
  }

  loadCategories() {
    this.catLoading = true;
    this.categoryService.getCategories().subscribe({
      next: cs => { this.categories = cs; this.catLoading = false; },
      error: () => this.catLoading = false
    });
  }

  onPageChange(p: number) { this.page = p; this.load(); }

  clearFilters() {
    this.filterStatus = undefined;
    this.filterPriority = undefined;
    this.filterCategoryId = undefined;
    this.search = '';
    this.load();
  }

  openNewTaskDrawer() {
    this.editingTask = undefined;
    this.taskForm = this.emptyTaskForm();
    this.drawerVisible = true;
  }

  openEditTaskDrawer(t: TaskItemDto) {
    this.editingTask = t;
    this.taskForm = {
      name: t.name,
      description: t.description ?? '',
      status: t.status,
      priority: t.priority,
      categoryId: t.categoryId ?? null,
      dueDate: t.dueDate ? new Date(t.dueDate) : null,
      tags: [...t.tags]
    };
    this.drawerVisible = true;
  }

  closeDrawer() { this.drawerVisible = false; }

  saveTask() {
    const payload: CreateTaskPayload = {
      name: this.taskForm.name.trim(),
      description: this.taskForm.description.trim() || undefined,
      status: this.taskForm.status as TaskItemStatus,
      priority: this.taskForm.priority as TaskPriority,
      categoryId: this.taskForm.categoryId ?? undefined,
      dueDate: this.taskForm.dueDate ? (this.taskForm.dueDate as Date).toISOString() : undefined,
      tags: this.taskForm.tags
    };

    const op = this.editingTask
      ? this.taskService.updateTask(this.editingTask.id, payload)
      : this.taskService.createTask(payload);

    op.subscribe({
      next: () => { this.closeDrawer(); this.load(); },
      error: () => this.msg.error('Failed to save task')
    });
  }

  deleteTask(id: string) {
    this.taskService.deleteTask(id).subscribe({
      next: () => this.load(),
      error: () => this.msg.error('Failed to delete task')
    });
  }

  // Category management
  saveCategory() {
    const name = this.categoryForm.name.trim();
    if (!name) return;
    this.categoryService.createCategory({ name }).subscribe({
      next: () => { this.categoryForm = { name: '' }; this.showCategoryForm = false; this.loadCategories(); },
      error: () => this.msg.error('Failed to create category')
    });
  }

  openCategoryForm() {
    this.categoryForm = { name: '' };
    this.showCategoryForm = true;
  }

  cancelCategoryForm() {
    this.showCategoryForm = false;
    this.categoryForm = { name: '' };
  }

  startCategoryEdit(c: CategoryDto) {
    this.editingCategoryId = c.id;
    this.categoryEditName = c.name;
  }

  saveInlineCategoryEdit(id: string) {
    const name = this.categoryEditName.trim();
    if (!name) return;
    this.categoryService.updateCategory(id, { name }).subscribe({
      next: () => { this.editingCategoryId = null; this.loadCategories(); },
      error: () => this.msg.error('Failed to update category')
    });
  }

  deleteCategory(id: string) {
    this.categoryService.deleteCategory(id).subscribe({
      next: () => this.loadCategories(),
      error: () => this.msg.error('Failed to delete category')
    });
  }

  logout() { this.authService.logout(); }

  isPastDue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }

  statusLabel(s: TaskItemStatus) { return ['Todo', 'In Progress', 'Done', 'Cancelled'][s]; }
  statusColor(s: TaskItemStatus) { return ['default', 'processing', 'success', 'error'][s]; }
  priorityLabel(p: TaskPriority) { return ['Low', 'Medium', 'High', 'Critical'][p]; }
  priorityColor(p: TaskPriority) { return ['default', 'blue', 'orange', 'red'][p]; }

  private emptyTaskForm() {
    return { name: '', description: '', status: 0, priority: 1, categoryId: null as string | null, dueDate: null as Date | null, tags: [] as string[] };
  }
}
