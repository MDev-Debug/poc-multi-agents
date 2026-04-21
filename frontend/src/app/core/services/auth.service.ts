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
	private readonly emailKey = 'userEmail';

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
		// sessionStorage é por-aba (permite múltiplos usuários em abas diferentes)
		sessionStorage.setItem(this.tokenKey, token);
		sessionStorage.setItem(this.refreshTokenKey, refreshToken);

		// Evita que tokens antigos em localStorage causem confusão.
		localStorage.removeItem(this.tokenKey);
		localStorage.removeItem(this.refreshTokenKey);
	}

	saveEmail(email: string): void {
		sessionStorage.setItem(this.emailKey, email);
	}

	getUserEmail(): string | null {
		return sessionStorage.getItem(this.emailKey);
	}

	getToken(): string | null {
		return sessionStorage.getItem(this.tokenKey);
	}

	clearTokens(): void {
		sessionStorage.removeItem(this.tokenKey);
		sessionStorage.removeItem(this.refreshTokenKey);
		sessionStorage.removeItem(this.emailKey);
		localStorage.removeItem(this.tokenKey);
		localStorage.removeItem(this.refreshTokenKey);
	}
}
