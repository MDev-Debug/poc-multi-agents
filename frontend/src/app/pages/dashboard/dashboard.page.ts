import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import { PresenceHubService } from '../../services/presence-hub.service';

@Component({
	selector: 'app-dashboard-page',
	imports: [CommonModule],
	templateUrl: './dashboard.page.html',
})
export class DashboardPage implements OnInit, OnDestroy {
	sidebarOpen = true;
	onlineUsers: string[] = [];

	private onlineUsersSub?: Subscription;

	constructor(
		private readonly auth: AuthService,
		private readonly presenceHub: PresenceHubService,
		private readonly router: Router,
	) {}

	ngOnInit(): void {
		// Conecta no hub para exibir usuários online na sidebar.
		void this.presenceHub.connect();
		this.onlineUsersSub = this.presenceHub.onlineUsers$.subscribe((users) => {
			this.onlineUsers = users.map((x) => x.email);
		});
	}

	ngOnDestroy(): void {
		this.onlineUsersSub?.unsubscribe();
		void this.presenceHub.disconnect();
	}

	toggleSidebar(): void {
		this.sidebarOpen = !this.sidebarOpen;
	}

	logout(): void {
		this.onlineUsersSub?.unsubscribe();
		void this.presenceHub.disconnect();
		this.auth.clearTokens();
		this.router.navigateByUrl('/auth');
	}
}
