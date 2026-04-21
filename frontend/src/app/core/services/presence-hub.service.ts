import { Injectable, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

export interface OnlineUser {
  userId: string;
  email: string;
  lastSeenAt: string;
}

@Injectable({ providedIn: 'root' })
export class PresenceHubService {
  private readonly auth = inject(AuthService);
  private connection: signalR.HubConnection | null = null;

  readonly onlineUsers$ = new BehaviorSubject<OnlineUser[]>([]);
  readonly connected$ = new BehaviorSubject<boolean>(false);

  async connect(): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/hubs/presence', {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('OnlineUsers', (users: OnlineUser[]) => {
      this.onlineUsers$.next(users ?? []);
    });

    this.connection.onreconnected(() => {
      this.connected$.next(true);
      this.connection?.invoke('GetOnlineUsers').catch(console.error);
    });

    this.connection.onclose(() => {
      this.connected$.next(false);
    });

    try {
      await this.connection.start();
      this.connected$.next(true);
      // After connecting, request current online users
      try {
        await this.connection.invoke('GetOnlineUsers');
      } catch (err) {
        console.error('[PresenceHub] Failed to invoke GetOnlineUsers:', err);
      }
    } catch (err) {
      console.error('[PresenceHub] Connection failed:', err);
      this.connected$.next(false);
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
        console.error('[PresenceHub] Disconnect error:', err);
      }
      this.connection = null;
      this.connected$.next(false);
      this.onlineUsers$.next([]);
    }
  }
}
