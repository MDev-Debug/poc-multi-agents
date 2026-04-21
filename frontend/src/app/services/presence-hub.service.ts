import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

import { AuthService } from './auth.service';

export type OnlineUser = {
	userId: string;
	email: string;
	lastSeenAt: string;
};

@Injectable({ providedIn: 'root' })
export class PresenceHubService {
	private readonly baseUrl = 'http://localhost:5000';
	private connection?: HubConnection;
	private readonly usersSubject = new BehaviorSubject<OnlineUser[]>([]);
	readonly onlineUsers$ = this.usersSubject.asObservable();

	constructor(private readonly auth: AuthService) {}

	async connect(): Promise<void> {
		const token = this.auth.getToken();
		if (!token) {
			this.usersSubject.next([]);
			return;
		}

		if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
			return;
		}

		this.connection = new HubConnectionBuilder()
			.withUrl(`${this.baseUrl}/hubs/presence`, {
				accessTokenFactory: () => this.auth.getToken() ?? '',
				withCredentials: false,
			})
			.withAutomaticReconnect()
			.configureLogging(LogLevel.Warning)
			.build();

		this.connection.on('OnlineUsers', (users: OnlineUser[]) => {
			this.usersSubject.next(users ?? []);
		});

		await this.connection.start();

		try {
			const users = await this.connection.invoke<OnlineUser[]>('GetOnlineUsers');
			this.usersSubject.next(users ?? []);
		} catch {
			// ignore
		}
	}

	async disconnect(): Promise<void> {
		this.usersSubject.next([]);

		if (!this.connection) {
			return;
		}

		try {
			await this.connection.stop();
		} catch {
			// ignore
		} finally {
			this.connection = undefined;
		}
	}
}
