import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { BookingsService } from '../../services/bookings.service';
import { SpacesService } from '../../services/spaces.service';
import { UsersService } from '../../services/users.service';
import { BookingResponse } from '../../models/booking-response';
import { getApiErrorMessage } from '../../utils/api-error.util';
import { fadeSlide, listStagger } from '../../shared/animations/app.animations';
import { StatusClassPipe, StatusLabelPipe } from '../../shared/pipes/status-label.pipe';
import { RevealDirective } from '../../shared/directives/reveal.directive';
@Component({
  selector: 'app-orders',
  imports: [
    DatePipe, RouterModule, MatButtonModule,
    MatCardModule, MatIconModule, StatusLabelPipe, StatusClassPipe, RevealDirective,
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
  animations: [fadeSlide, listStagger],
})
export class Orders implements OnInit {
  private readonly router = inject(Router);
  readonly bookingsService = inject(BookingsService);
  readonly spacesService = inject(SpacesService);
  readonly usersService = inject(UsersService);

  readonly cancellingId = signal<string | null>(null);
  readonly cancelError = signal<string | null>(null);

  ngOnInit(): void {
    if (!this.usersService.isAuthenticated || this.usersService.isAdmin) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    this.spacesService.ensureCatalog();

    const userId = this.usersService.currentUserId;
    if (userId) {
      this.bookingsService.loadBookingsByUser(userId);
    }
  }

  spaceName(spaceId: string): string {
    return this.spacesService.nameById(spaceId);
  }

  spaceLocation(spaceId: string): string {
    return this.spacesService.locationById(spaceId);
  }

  reloadBookings(): void {
    const userId = this.usersService.currentUserId;
    if (userId) {
      this.bookingsService.loadBookingsByUser(userId);
    }
  }

  canCancel(booking: BookingResponse): boolean {
    return booking.status.toLowerCase() !== 'cancelled';
  }

  cancelBooking(booking: BookingResponse): void {
    if (!this.canCancel(booking) || this.cancellingId()) {
      return;
    }

    this.cancellingId.set(booking.id);
    this.cancelError.set(null);

    this.bookingsService.cancelBooking(booking.id).subscribe({
      next: () => {
        this.bookingsService.markCancelled(booking.id);
        const userId = this.usersService.currentUserId;
        if (userId) {
          this.bookingsService.invalidateByUser(userId);
        }
        this.cancellingId.set(null);
      },
      error: (error) => {
        this.cancellingId.set(null);
        this.cancelError.set(getApiErrorMessage(error, 'Não foi possível cancelar a reserva.'));
      },
    });
  }
}
