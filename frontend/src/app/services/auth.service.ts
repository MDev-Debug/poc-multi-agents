import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

type AuthResponse = {
	userId: string;
	email: string;
	token: string;
	refreshToken: string;
};

@Injectable({ providedIn: 'root' })
export class AuthService {
	private readonly baseUrl = 'http://localhost:5000';
	private readonly tokenKey = 'chat_token';
	private readonly refreshTokenKey = 'chat_refresh_token';

	constructor(private readonly http: HttpClient) {}

	register(email: string, password: string): Observable<AuthResponse> {
		return this.http.post<AuthResponse>(`${this.baseUrl}/api/auth/register`, {
			email,
			password,
		});
	}

	login(email: string, password: string): Observable<AuthResponse> {
		return this.http.post<AuthResponse>(`${this.baseUrl}/api/auth/login`, {
			email,
			password,
		});
	}

	saveTokens(token: string, refreshToken: string): void {
		localStorage.setItem(this.tokenKey, token);
		localStorage.setItem(this.refreshTokenKey, refreshToken);
	}

	getToken(): string | null {
		return localStorage.getItem(this.tokenKey);
	}

	clearTokens(): void {
		localStorage.removeItem(this.tokenKey);
		localStorage.removeItem(this.refreshTokenKey);
	}
}
