import { Routes } from '@angular/router';

import { AuthPage } from './pages/auth/auth.page';
import { DashboardPage } from './pages/dashboard/dashboard.page';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'auth' },
	{ path: 'auth', component: AuthPage },
	{ path: 'dashboard', component: DashboardPage, canActivate: [authGuard] },
	{ path: '**', redirectTo: 'auth' },
];
