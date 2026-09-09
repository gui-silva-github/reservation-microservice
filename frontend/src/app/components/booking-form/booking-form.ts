import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SpacesService } from '../../services/spaces.service';
import { BookingsService } from '../../services/bookings.service';
import { UsersService } from '../../services/users.service';
import { SpaceResponse } from '../../models/space-response';
import {
  applyApiFieldErrors,
  clearApiFieldErrors,
  getApiError,
  getApiErrorMessage,
  hasApiError,
  parseApiError
} from '../../utils/api-error.util';

@Component({
  selector: 'app-booking-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './booking-form.html',
  styleUrl: './booking-form.css',
})
export class BookingForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly spacesService = inject(SpacesService);
  private readonly bookingsService = inject(BookingsService);
  private readonly usersService = inject(UsersService);
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly space = signal<SpaceResponse | null>(null);

  bookingForm: FormGroup = this.fb.group({
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
  });

  ngOnInit(): void {
    if (!this.usersService.isAuthenticated || this.usersService.isAdmin) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    const spaceId = this.activatedRoute.snapshot.paramMap.get('id') ?? '';
    if (!spaceId) {
      this.loading.set(false);
      this.error.set('ID do espaço inválido');
      return;
    }

    this.spacesService.getSpaceById(spaceId).subscribe({
      next: (response) => {
        this.space.set(response);
        this.loading.set(false);
        this.error.set(null);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Espaço não encontrado ou serviço indisponível.');
      }
    });
  }

  submit(): void {
    if (!this.bookingForm.valid || this.isSubmitting() || !this.space()) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    const userId = this.usersService.currentUserId;
    if (!userId) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    const { startDate, endDate } = this.bookingForm.value;
    if (new Date(endDate) <= new Date(startDate)) {
      this.errorMessage.set('A data de término deve ser posterior à data de início.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    clearApiFieldErrors(this.bookingForm);

    this.bookingsService
      .createBooking({
        userId,
        spaceId: this.space()!.id,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigate(['/bookings']);
        },
        error: (error) => {
          this.isSubmitting.set(false);
          const parsed = parseApiError(error);
          applyApiFieldErrors(this.bookingForm, parsed.errors, {
            StartDate: 'startDate',
            EndDate: 'endDate',
          });
          this.errorMessage.set(getApiErrorMessage(error, 'Não foi possível criar a reserva.'));
        }
      });
  }

  get startDateFormControl(): FormControl {
    return this.bookingForm.get('startDate') as FormControl;
  }

  get endDateFormControl(): FormControl {
    return this.bookingForm.get('endDate') as FormControl;
  }

  protected readonly hasApiError = hasApiError;
  protected readonly getApiError = getApiError;
}
