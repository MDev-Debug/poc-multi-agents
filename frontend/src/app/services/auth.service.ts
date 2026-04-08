import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

type AuthResponse = {
	userId: string;
	email: string;
	token: string;
};

@Injectable({ providedIn: 'root' })
export class AuthService {
	private readonly baseUrl = 'http://localhost:5000';
	private readonly tokenKey = 'chat_token';

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

	saveToken(token: string): void {
		localStorage.setItem(this.tokenKey, token);
	}

	getToken(): string | null {
		return localStorage.getItem(this.tokenKey);
	}
}
