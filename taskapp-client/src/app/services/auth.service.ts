import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

interface LoginRequest {
  username: string;
  password: string;
}

interface AuthUser {
  username: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly currentUser = signal<string | null>(null);
  readonly isAuthenticated = signal<boolean>(false);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  constructor(private readonly http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthUser> {
    return this.http
      .post<AuthUser>(`${this.baseUrl}/login`, payload, { withCredentials: true })
      .pipe(
        tap((result) => {
          this.currentUser.set(result.username);
          this.isAuthenticated.set(true);
        })
      );
  }

  logout(): Observable<unknown> {
    return this.http
      .post(`${this.baseUrl}/logout`, {}, { withCredentials: true })
      .pipe(
        tap(() => {
          this.currentUser.set(null);
          this.isAuthenticated.set(false);
        })
      );
  }

  me(): Observable<AuthUser> {
    return this.http.get<AuthUser>(`${this.baseUrl}/me`, { withCredentials: true }).pipe(
      tap((result) => {
        this.currentUser.set(result.username);
        this.isAuthenticated.set(true);
      })
    );
  }

  clearAuth(): void {
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }
}
