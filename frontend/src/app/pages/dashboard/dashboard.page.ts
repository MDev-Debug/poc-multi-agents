import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
	selector: 'app-dashboard-page',
	imports: [CommonModule],
	templateUrl: './dashboard.page.html',
})
export class DashboardPage {
	sidebarOpen = true;
	active: 'home' | 'chat' = 'home';

	constructor(
		private readonly auth: AuthService,
		private readonly router: Router,
	) {}

	toggleSidebar(): void {
		this.sidebarOpen = !this.sidebarOpen;
	}

	setActive(view: 'home' | 'chat'): void {
		this.active = view;
	}

	logout(): void {
		this.auth.clearTokens();
		this.router.navigateByUrl('/auth');
	}
}
