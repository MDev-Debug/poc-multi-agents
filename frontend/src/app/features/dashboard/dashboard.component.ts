import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { PresenceHubService } from '../../core/services/presence-hub.service';
import { OnlineUser } from '../../core/services/presence-hub.service';
import { ChatHubService } from '../../core/services/chat-hub.service';
import { UnreadMessagesService } from '../../core/services/unread-messages.service';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { OnlineUsersComponent } from './components/online-users/online-users.component';
import { PrivateChatComponent } from './components/private-chat/private-chat.component';

export type DashboardView = 'home' | 'chat';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    SidebarComponent,
    TopbarComponent,
    OnlineUsersComponent,
    PrivateChatComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  readonly presenceHub = inject(PresenceHubService);
  private readonly chatHub = inject(ChatHubService);
  readonly unreadMessages = inject(UnreadMessagesService);

  currentView = signal<DashboardView>('chat');
  activeConversationUser = signal<OnlineUser | null>(null);
  emptyMap = new Map<string, number>();

  private subs = new Subscription();

  get userEmail(): string {
    return this.auth.getUserEmail() ?? 'user';
  }

  get userInitial(): string {
    return this.userEmail.charAt(0).toUpperCase();
  }

  ngOnInit(): void {
    this.presenceHub.connect();
    this.chatHub.connect();

    this.subs.add(
      this.chatHub.messages$.subscribe(msg => {
        const activeUser = this.activeConversationUser();
        if (!activeUser || activeUser.userId !== msg.senderId) {
          if (msg.senderId !== this.auth.getUserId()) {
            this.unreadMessages.increment(msg.senderId);
          }
        }
      })
    );
  }

  setView(view: DashboardView): void {
    this.currentView.set(view);
  }

  openConversation(user: OnlineUser): void {
    this.activeConversationUser.set(user);
    this.unreadMessages.clear(user.userId);
  }

  closeConversation(): void {
    this.activeConversationUser.set(null);
  }

  async logout(): Promise<void> {
    await this.presenceHub.disconnect();
    await this.chatHub.disconnect();
    this.auth.clearTokens();
    this.router.navigate(['/auth']);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.presenceHub.disconnect();
    this.chatHub.disconnect();
  }
}
