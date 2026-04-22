import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

export interface ChatMessage {
  messageId: string;
  senderId: string;
  senderEmail: string;
  content: string;
  sentAt: string; // ISO 8601
}

@Injectable({ providedIn: 'root' })
export class ChatHubService {
  private readonly auth = inject(AuthService);
  private connection: signalR.HubConnection | null = null;

  readonly messages$ = new Subject<ChatMessage>();
  readonly connected$ = new BehaviorSubject<boolean>(false);

  async connect(): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/chat`, {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('ReceiveMessage', (message: ChatMessage) => {
      this.messages$.next(message);
    });

    this.connection.onreconnected(() => {
      this.connected$.next(true);
    });

    this.connection.onclose(() => {
      this.connected$.next(false);
    });

    try {
      await this.connection.start();
      this.connected$.next(true);
    } catch (err) {
      console.error('[ChatHub] Connection failed:', err);
      this.connected$.next(false);
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
        console.error('[ChatHub] Disconnect error:', err);
      }
      this.connection = null;
      this.connected$.next(false);
    }
  }

  async sendMessage(receiverId: string, content: string): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('ChatHub not connected');
    }
    await this.connection.invoke('SendPrivateMessage', receiverId, content);
  }
}
