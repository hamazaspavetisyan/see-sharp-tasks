import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/tasks', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./auth/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./auth/register.component').then(m => m.RegisterComponent) },
  { path: 'tasks', loadComponent: () => import('./tasks/task-list.component').then(m => m.TaskListComponent), canActivate: [authGuard] },
  { path: '**', redirectTo: '/tasks' }
];
