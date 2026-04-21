import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { PresenceHubService } from '../../core/services/presence-hub.service';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { OnlineUsersComponent } from './components/online-users/online-users.component';

export type DashboardView = 'home' | 'chat';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, SidebarComponent, TopbarComponent, OnlineUsersComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  readonly presenceHub = inject(PresenceHubService);

  currentView = signal<DashboardView>('chat');
  private subs = new Subscription();

  get userEmail(): string {
    return this.auth.getUserEmail() ?? 'user';
  }

  get userInitial(): string {
    return this.userEmail.charAt(0).toUpperCase();
  }

  ngOnInit(): void {
    this.presenceHub.connect();
  }

  setView(view: DashboardView): void {
    this.currentView.set(view);
  }

  async logout(): Promise<void> {
    await this.presenceHub.disconnect();
    this.auth.clearTokens();
    this.router.navigate(['/auth']);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.presenceHub.disconnect();
  }
}
