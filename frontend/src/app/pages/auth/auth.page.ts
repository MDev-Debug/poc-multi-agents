import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';

type AuthMode = 'login' | 'register';

@Component({
	selector: 'app-auth-page',
	imports: [CommonModule, ReactiveFormsModule],
	templateUrl: './auth.page.html',
})
export class AuthPage {
	mode: AuthMode = 'login';
	loading = false;
	errorMessage = '';
	okMessage = '';
	form;

	constructor(
		private readonly fb: FormBuilder,
		private readonly auth: AuthService,
		private readonly router: Router,
	) {
		this.form = this.fb.nonNullable.group({
			email: ['', [Validators.required, Validators.email]],
			password: ['', [Validators.required, Validators.minLength(6)]],
		});
	}

	setMode(mode: AuthMode): void {
		this.mode = mode;
		this.errorMessage = '';
		this.okMessage = '';
	}

	submit(): void {
		this.errorMessage = '';
		this.okMessage = '';

		if (this.form.invalid) {
			this.form.markAllAsTouched();
			this.errorMessage = 'Revise os campos e tente novamente.';
			return;
		}

		this.loading = true;
		const { email, password } = this.form.getRawValue();

		const request$ =
			this.mode === 'login'
				? this.auth.login(email, password)
				: this.auth.register(email, password);

		request$
			.pipe(finalize(() => (this.loading = false)))
			.subscribe({
				next: (res) => {
					this.auth.saveTokens(res.token, res.refreshToken);
					this.okMessage = this.mode === 'login' ? 'Login realizado.' : 'Cadastro realizado.';
					this.router.navigateByUrl('/dashboard');
				},
				error: (err) => {
					this.errorMessage =
						err?.error?.message ?? 'Não foi possível concluir. Tente novamente.';
				},
			});
	}
}
