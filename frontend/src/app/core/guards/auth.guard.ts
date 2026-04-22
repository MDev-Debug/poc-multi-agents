import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
	const auth = inject(AuthService);
	const router = inject(Router);

	const token = auth.getToken();
	if (!token) {
		router.navigateByUrl('/auth');
		return false;
	}

	// Verificar se o token JWT está expirado (parse do payload Base64)
	try {
		const payload = JSON.parse(atob(token.split('.')[1]));
		const expMs = payload.exp * 1000;
		if (Date.now() >= expMs) {
			// Token expirado — limpar e redirecionar
			auth.clearTokens();
			router.navigateByUrl('/auth');
			return false;
		}
	} catch {
		// Token malformado
		auth.clearTokens();
		router.navigateByUrl('/auth');
		return false;
	}

	return true;
};
