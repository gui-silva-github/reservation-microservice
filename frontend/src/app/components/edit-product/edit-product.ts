import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UsersService } from '../../services/users.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SpacesService } from '../../services/spaces.service';
import { SpaceResponse } from '../../models/space-response';
import {
  applyApiFieldErrors,
  clearApiFieldErrors,
  getApiErrorMessage,
  parseApiError,
} from '../../utils/api-error.util';

@Component({
  selector: 'app-edit-product',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './edit-product.html',
  styleUrl: './edit-product.css',
})
export class EditProduct implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly spacesService = inject(SpacesService);
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  readonly usersService = inject(UsersService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly saveError = signal<string | null>(null);
  readonly saving = signal(false);

  editSpaceForm: FormGroup = this.fb.group({
    id: ['', Validators.required],
    name: ['', Validators.required],
    location: ['', Validators.required],
    description: [''],
    capacity: [1, [Validators.required, Validators.min(1)]],
    pricePerHour: [0, [Validators.required, Validators.min(0)]],
  });

  ngOnInit(): void {
    const id = this.activatedRoute.snapshot.paramMap.get('id') ?? '';

    if (!id) {
      this.loading.set(false);
      this.error.set('ID do espaço inválido.');
      return;
    }

    const cached = this.spacesService.catalog().find((s) => s.id === id);
    if (cached) {
      this.fillForm(cached);
      this.loading.set(false);
    }

    this.spacesService.getSpaceById(id).subscribe({
      next: (response) => {
        this.fillForm(response);
        this.loading.set(false);
        this.error.set(null);
      },
      error: () => {
        if (!cached) {
          this.loading.set(false);
          this.error.set('Espaço não encontrado ou serviço indisponível.');
        }
      },
    });
  }

  update(): void {
    if (!this.editSpaceForm.valid || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);
    clearApiFieldErrors(this.editSpaceForm);

    const { id, name, location, description, capacity, pricePerHour } = this.editSpaceForm.getRawValue();

    this.spacesService.updateSpace(id, { name, location, description, capacity, pricePerHour }).subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/admin', 'spaces']);
      },
      error: (error) => {
        this.saving.set(false);

        const parsed = parseApiError(error);
        applyApiFieldErrors(this.editSpaceForm, parsed.errors, {
          Name: 'name',
          Location: 'location',
          Description: 'description',
          Capacity: 'capacity',
          PricePerHour: 'pricePerHour',
        });
        this.saveError.set(getApiErrorMessage(error, 'Erro ao salvar as alterações. Tente novamente.'));
      },
    });
  }

  private fillForm(space: SpaceResponse): void {
    this.editSpaceForm.patchValue({
      id: space.id,
      name: space.name,
      location: space.location,
      description: space.description ?? '',
      capacity: space.capacity,
      pricePerHour: space.pricePerHour,
    });
  }

  get nameFormControl(): FormControl {
    return this.editSpaceForm.get('name') as FormControl;
  }

  get locationFormControl(): FormControl {
    return this.editSpaceForm.get('location') as FormControl;
  }
}
