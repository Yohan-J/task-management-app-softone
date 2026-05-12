import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { TaskService } from '../../services/task.service';
import { TaskItem, TaskUpsertRequest } from '../../models/task.model';
import { BaseComponent } from '../../shared/base.component';

@Component({
  selector: 'app-tasks',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent extends BaseComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  readonly tasks = signal<TaskItem[]>([]);
  readonly selectedTaskId = signal<number | null>(null);
  readonly isLoading = signal(false);
  readonly formError = signal('');
  readonly listError = signal('');
  readonly currentPage = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly totalPages = computed(() => {
    const count = this.totalCount();
    const size = this.pageSize();
    if (count === 0) {
      return 1;
    }
    return Math.max(1, Math.ceil(count / size));
  });

  /** Sorted unique page indices to show as numeric buttons (with gaps rendered between jumps). */
  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const cur = this.currentPage();
    if (total <= 1) {
      return [1];
    }
    const maxButtons = 7;
    if (total <= maxButtons) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }
    const set = new Set<number>([1, total]);
    for (let d = -2; d <= 2; d++) {
      const p = cur + d;
      if (p >= 1 && p <= total) {
        set.add(p);
      }
    }
    return Array.from(set).sort((a, b) => a - b);
  });

  readonly filterForm = this.formBuilder.group({
    search: [''],
    status: ['all'],
    priority: ['all'],
    sortBy: ['createdAt'],
    sortOrder: ['desc']
  });

  readonly taskForm = this.formBuilder.group({
    title: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
    priority: ['Medium', [Validators.required]],
    dueDate: [''],
    isCompleted: [false]
  });

  taskService = inject(TaskService);
  authService = inject(AuthService);

  constructor(
    private readonly router: Router,
  ) {
    super();
  }

  get editingTask(): TaskItem | undefined {
    const id = this.selectedTaskId();
    return this.tasks().find((item) => item.id === id);
  }

  ngOnInit(): void {
    this.loadTasks();
  }

  applyFilters(): void {
    this.currentPage.set(1);
    this.loadTasks();
  }

  loadTasks(): void {
    this.listError.set('');
    this.isLoading.set(true);
    const filter = this.filterForm.getRawValue();

    this.taskService
      .getTasks({
        search: filter.search || undefined,
        status: filter.status !== 'all' ? filter.status || undefined : undefined,
        priority: filter.priority !== 'all' ? filter.priority || undefined : undefined,
        sortBy: filter.sortBy || 'createdAt',
        sortOrder: filter.sortOrder || 'desc',
        page: this.currentPage(),
        pageSize: this.pageSize()
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .pipe(takeUntil(this.destroyed$))
      .subscribe({
        next: (result) => {
          const rows = result.items ?? [];
          if (rows.length === 0 && result.totalCount > 0 && this.currentPage() > 1) {
            this.currentPage.set(this.currentPage() - 1);
            this.loadTasks();
            return;
          }
          this.tasks.set(rows);
          this.totalCount.set(result.totalCount);
        },
        error: () => this.listError.set('Unable to load tasks.')
      });
  }

  goToPage(page: number): void {
    const total = this.totalPages();
    const target = Math.min(Math.max(1, Math.floor(page)), total);
    if (target === this.currentPage()) {
      return;
    }
    this.currentPage.set(target);
    this.loadTasks();
  }

  goToPreviousPage(): void {
    this.goToPage(this.currentPage() - 1);
  }

  goToNextPage(): void {
    this.goToPage(this.currentPage() + 1);
  }

  selectTask(task: TaskItem): void {
    this.selectedTaskId.set(task.id);
    this.taskForm.patchValue({
      title: task.title,
      description: task.description ?? '',
      priority: task.priority,
      dueDate: task.dueDate ? task.dueDate.slice(0, 10) : '',
      isCompleted: task.isCompleted
    });
  }

  clearForm(): void {
    this.selectedTaskId.set(null);
    this.formError.set('');
    this.taskForm.reset({
      title: '',
      description: '',
      priority: 'Medium',
      dueDate: '',
      isCompleted: false
    });
  }

  hasTaskControlError(controlName: keyof TaskUpsertRequest, errorName: string): boolean {
    const control = this.taskForm.get(controlName);
    return !!(control && control.touched && control.hasError(errorName));
  }

  saveTask(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.formError.set('');
    const raw = this.taskForm.getRawValue();
    const payload: TaskUpsertRequest = {
      title: raw.title || '',
      description: raw.description || '',
      priority: (raw.priority || 'Medium') as 'Low' | 'Medium' | 'High',
      dueDate: raw.dueDate || null,
      isCompleted: !!raw.isCompleted
    };

    const editingId = this.selectedTaskId();
    const request$ = editingId
      ? this.taskService.update(editingId, payload)
      : this.taskService.create(payload);

    request$.pipe(takeUntil(this.destroyed$)).subscribe({
      next: () => {
        this.clearForm();
        this.loadTasks();
      },
      error: () => this.formError.set('Unable to save task.')
    });
  }

  deleteTask(taskId: number): void {
    this.taskService.delete(taskId).pipe(takeUntil(this.destroyed$)).subscribe({
      next: () => {
        if (this.selectedTaskId() === taskId) {
          this.clearForm();
        }
        this.loadTasks();
      },
      error: () => this.listError.set('Unable to delete task.')
    });
  }

  markCompleted(taskId: number): void {
    this.taskService.markCompleted(taskId).pipe(takeUntil(this.destroyed$)).subscribe({
      next: () => this.loadTasks(),
      error: () => this.listError.set('Unable to mark task complete.')
    });
  }

  logout(): void {
    this.authService.logout().pipe(takeUntil(this.destroyed$)).subscribe({
      next: () => this.router.navigateByUrl('/login')
    });
  }
}
