import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { BookingsService } from '../../services/bookings.service';
import { UsersService } from '../../services/users.service';
import { BookingResponse } from '../../models/booking-response';
import { getApiErrorMessage } from '../../utils/api-error.util';

@Component({
  selector: 'app-orders',
  imports: [
    DatePipe, RouterModule, MatButtonModule,
    MatCardModule, MatIconModule
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders implements OnInit {
  private readonly router = inject(Router);
  readonly bookingsService = inject(BookingsService);
  readonly usersService = inject(UsersService);

  readonly cancellingId = signal<string | null>(null);
  readonly cancelError = signal<string | null>(null);

  ngOnInit(): void {
    if (!this.usersService.isAuthenticated || this.usersService.isAdmin) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    const userId = this.usersService.currentUserId;
    if (userId) {
      this.bookingsService.loadBookingsByUser(userId);
    }
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
        this.bookingsService.removeBookingFromList(booking.id);
        this.cancellingId.set(null);
      },
      error: (error) => {
        this.cancellingId.set(null);
        this.cancelError.set(getApiErrorMessage(error, 'Não foi possível cancelar a reserva.'));
      },
    })
  }
}
