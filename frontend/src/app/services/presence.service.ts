import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type OnlineUser = {
	userId: string;
	email: string;
	lastSeenAt: string;
};

@Injectable({ providedIn: 'root' })
export class PresenceService {
	private readonly baseUrl = 'http://localhost:5000';

	constructor(private readonly http: HttpClient) {}

	heartbeat(): Observable<void> {
		return this.http.post<void>(`${this.baseUrl}/api/presence/heartbeat`, {});
	}

	getOnline(): Observable<OnlineUser[]> {
		return this.http.get<OnlineUser[]>(`${this.baseUrl}/api/presence/online`);
	}
}
