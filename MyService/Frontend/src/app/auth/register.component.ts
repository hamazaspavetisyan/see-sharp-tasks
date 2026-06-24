import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NzFormModule, NzInputModule, NzButtonModule, NzCardModule],
  template: `
    <div style="display:flex;justify-content:center;align-items:center;height:100vh;background:#f0f2f5">
      <nz-card style="width:380px" nzTitle="Create Account">
        <form nz-form nzLayout="vertical" (ngSubmit)="onSubmit()">
          <nz-form-item>
            <nz-form-label nzRequired>Email</nz-form-label>
            <nz-form-control>
              <input nz-input [(ngModel)]="email" name="email" type="email" placeholder="you@example.com" required />
            </nz-form-control>
          </nz-form-item>
          <nz-form-item>
            <nz-form-label nzRequired>Password</nz-form-label>
            <nz-form-control>
              <input nz-input [(ngModel)]="password" name="password" type="password" placeholder="Min 6 chars" required />
            </nz-form-control>
          </nz-form-item>
          <button nz-button nzType="primary" nzBlock [nzLoading]="loading" type="submit">Create Account</button>
          <p style="margin-top:12px;text-align:center">
            Already have an account? <a routerLink="/login">Sign in</a>
          </p>
        </form>
      </nz-card>
    </div>
  `
})
export class RegisterComponent {
  email = '';
  password = '';
  loading = false;
  private auth = inject(AuthService);
  private router = inject(Router);
  private msg = inject(NzMessageService);

  onSubmit() {
    this.loading = true;
    this.auth.register(this.email, this.password).subscribe({
      next: () => {
        this.msg.success('Account created! Please sign in.');
        this.router.navigate(['/login']);
      },
      error: () => {
        this.msg.error('Registration failed. Email may already be in use.');
        this.loading = false;
      }
    });
  }
}
