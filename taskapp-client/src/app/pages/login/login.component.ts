import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { BaseComponent } from '../../shared/base.component';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent extends BaseComponent {
  private readonly formBuilder = inject(FormBuilder);
  errorMessage = '';
  isSubmitting = false;

  readonly form = this.formBuilder.group({
    username: ['admin', [Validators.required]],
    password: ['password123', [Validators.required]]
  });

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    super();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.isSubmitting = true;

    this.authService
      .login(this.form.getRawValue() as { username: string; password: string })
      .pipe(finalize(() => (this.isSubmitting = false)))
      .pipe(takeUntil(this.destroyed$))
      .subscribe({
        next: () => this.router.navigateByUrl('/tasks'),
        error: () => (this.errorMessage = 'Invalid username or password.')
      });
  }
}
