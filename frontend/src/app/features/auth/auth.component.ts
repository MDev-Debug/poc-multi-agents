import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

type AuthMode = 'login' | 'register';

// Validator customizado para complexidade mínima de senha
function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
	const value: string = control.value ?? '';
	if (value.length < 8) return null; // defer to minLength

	const hasUpper  = /[A-Z]/.test(value);
	const hasLower  = /[a-z]/.test(value);
	const hasNumber = /[0-9]/.test(value);

	if (!hasUpper || !hasLower || !hasNumber) {
		return { passwordStrength: 'A senha deve conter letras maiúsculas, minúsculas e números.' };
	}
	return null;
}

@Component({
	selector: 'app-auth',
	standalone: true,
	imports: [CommonModule, ReactiveFormsModule],
	templateUrl: './auth.component.html',
	styleUrl: './auth.component.scss',
})
export class AuthComponent {
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
			email: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
			password: [
				'',
				[
					Validators.required,
					Validators.minLength(8),       // aumentado de 6 para 8
					Validators.maxLength(128),     // prevenir DoS no hash
					passwordStrengthValidator,     // complexidade mínima
				]
			],
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
					this.auth.saveEmail(res.email ?? email);
					this.auth.saveUserId(res.userId);
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
