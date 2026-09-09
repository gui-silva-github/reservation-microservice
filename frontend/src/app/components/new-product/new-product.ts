import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UsersService } from '../../services/users.service';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SpacesService } from '../../services/spaces.service';
import { SpaceResponse } from '../../models/space-response';
import {
  applyApiFieldErrors,
  clearApiFieldErrors,
  getApiError,
  getApiErrorMessage,
  hasApiError,
  parseApiError,
} from '../../utils/api-error.util';

@Component({
  selector: 'app-new-product',
  imports: [
    CommonModule, ReactiveFormsModule, MatButtonModule, MatIconModule, MatToolbarModule, RouterModule, MatCardModule,
    MatFormFieldModule, MatInputModule,
  ],
  templateUrl: './new-product.html',
  styleUrl: './new-product.css',
})
export class NewProduct {
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  newSpaceForm: FormGroup;

  constructor(private fb: FormBuilder, public usersService: UsersService, private spacesService: SpacesService, private router: Router) {
    this.newSpaceForm = this.fb.group({
      name: ['', [Validators.required]],
      location: ['', [Validators.required]],
      description: [''],
      capacity: [1, [Validators.required, Validators.min(1)]],
      pricePerHour: [0, [Validators.required, Validators.min(0)]],
    });
  }

  create(): void {
    if (!this.newSpaceForm.valid || this.isSubmitting()) {
      this.newSpaceForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    clearApiFieldErrors(this.newSpaceForm);

    this.spacesService.createSpace(this.newSpaceForm.value).subscribe({
      next: (response: SpaceResponse) => {
        this.isSubmitting.set(false);
        if (response) {
          this.router.navigate(['/admin', 'spaces']);
        }
      },
      error: (error) => {
        this.isSubmitting.set(false);

        const parsed = parseApiError(error);
        applyApiFieldErrors(this.newSpaceForm, parsed.errors, {
          Name: 'name',
          Location: 'location',
          Description: 'description',
          Capacity: 'capacity',
          PricePerHour: 'pricePerHour',
        });
        this.errorMessage.set(getApiErrorMessage(error, 'Não foi possível criar o espaço.'));
      },
    });
  }

  get nameFormControl(): FormControl {
    return this.newSpaceForm.get('name') as FormControl;
  }

  get locationFormControl(): FormControl {
    return this.newSpaceForm.get('location') as FormControl;
  }

  get descriptionFormControl(): FormControl {
    return this.newSpaceForm.get('description') as FormControl;
  }

  get capacityFormControl(): FormControl {
    return this.newSpaceForm.get('capacity') as FormControl;
  }

  get pricePerHourFormControl(): FormControl {
    return this.newSpaceForm.get('pricePerHour') as FormControl;
  }

  protected readonly hasApiError = hasApiError;
  protected readonly getApiError = getApiError;
}
