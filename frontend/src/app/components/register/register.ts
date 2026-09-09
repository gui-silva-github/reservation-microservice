import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthenticationResponse } from '../../models/authentication-response';
import { UsersService } from '../../services/users.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import {
  applyApiFieldErrors,
  clearApiFieldErrors,
  getApiError,
  getApiErrorMessage,
  hasApiError,
  parseApiError,
} from '../../utils/api-error.util';

@Component({
  selector: 'app-register',
  imports: [
    CommonModule, ReactiveFormsModule, MatButtonModule, MatIconModule, MatToolbarModule,
    RouterModule, MatCardModule, MatFormFieldModule, MatInputModule,
  ],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  registerForm: FormGroup;

  constructor(private fb: FormBuilder, private usersService: UsersService, private router: Router) {
    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  register(): void {
    if (!this.registerForm.valid || this.isSubmitting()) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    clearApiFieldErrors(this.registerForm);

    this.usersService.register(this.registerForm.value).subscribe({
      next: (response: AuthenticationResponse) => {
        this.isSubmitting.set(false);
        const auth = this.usersService.normalizeAuthResponse(response as unknown as Record<string, unknown>);
        const isAdmin = this.usersService.isAdminRole(auth.role);
        this.usersService.setAuthStatus(auth.token, isAdmin, auth.name, auth.userId);
        this.router.navigate(['/spaces']);
      },
      error: (error) => {
        this.isSubmitting.set(false);

        const parsed = parseApiError(error);
        applyApiFieldErrors(this.registerForm, parsed.errors, {
          Name: 'name',
          Email: 'email',
          Password: 'password',
        });
        this.errorMessage.set(getApiErrorMessage(error, 'Não foi possível concluir o cadastro.'));
      },
    });
  }

  get emailFormControl(): FormControl {
    return this.registerForm.get('email') as FormControl;
  }

  get passwordFormControl(): FormControl {
    return this.registerForm.get('password') as FormControl;
  }

  get nameFormControl(): FormControl {
    return this.registerForm.get('name') as FormControl;
  }

  protected readonly hasApiError = hasApiError;
  protected readonly getApiError = getApiError;
}
