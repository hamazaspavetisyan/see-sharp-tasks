import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginResponse, RegisterResponse, UserDto } from '../shared/models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly TOKEN_KEY = 'auth_token';

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password }).pipe(
      tap(res => localStorage.setItem(this.TOKEN_KEY, res.token))
    );
  }

  register(email: string, password: string): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>('/api/auth/register', { email, password });
  }

  getMe(): Observable<UserDto> {
    return this.http.get<UserDto>('/api/auth/me');
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.router.navigate(['/login']);
  }
}
