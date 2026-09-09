import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environment';
import { BookingResponse } from '../models/booking-response';
import { CreateBookingRequest } from '../models/create-booking-request';

@Injectable({
  providedIn: 'root'
})
export class BookingsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.bookingsUrl.replace(/\/$/, '');

  readonly bookings = signal<BookingResponse[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly hasBookings = computed(() => this.bookings().length > 0);

  private authHeaders(): HttpHeaders {
    const token = localStorage.getItem('authToken');
    return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
  }

  getBookingsByUserId(userId: string): Observable<BookingResponse[]> {
    return this.http
      .get<unknown>(`${this.baseUrl}/search/userid/${userId}`)
      .pipe(map((data) => this.toBookings(data)));
  }

  createBooking(request: CreateBookingRequest): Observable<BookingResponse> {
    return this.http
      .post<unknown>(this.baseUrl, request, { headers: this.authHeaders() })
      .pipe(map((data) => this.toBooking(data as Record<string, unknown>)));
  }

  cancelBooking(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { headers: this.authHeaders() });
  }

  loadBookingsByUser(userId: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.getBookingsByUserId(userId).subscribe({
      next: (bookings) => {
        this.bookings.set(this.sortByDateDesc(bookings));
        this.loading.set(false);
      },
      error: () => {
        this.bookings.set([]);
        this.error.set('Não foi possível carregar as reservas. Verifique se o microserviço está ativo.');
        this.loading.set(false);
      }
    });
  }

  removeBookingFromList(id: string): void {
    this.bookings.update((items) => items.filter((b) => b.id !== id));
  }

  private sortByDateDesc(bookings: BookingResponse[]): BookingResponse[] {
    return [...bookings].sort(
      (first, second) => new Date(second.startDate).getTime() - new Date(first.startDate).getTime()
    );
  }

  private toBookings(data: unknown): BookingResponse[] {
    if (!data) {
      return [];
    }

    if (!Array.isArray(data)) {
      return [this.toBooking(data as Record<string, unknown>)];
    }

    return data
      .filter((item) => item != null)
      .map((item) => this.toBooking(item as Record<string, unknown>));
  }

  private toBooking(raw: Record<string, unknown>): BookingResponse {
    return {
      id: String(raw['id'] ?? raw['Id'] ?? ''),
      userId: String(raw['userId'] ?? raw['UserId'] ?? ''),
      spaceId: String(raw['spaceId'] ?? raw['SpaceId'] ?? ''),
      startDate: String(raw['startDate'] ?? raw['StartDate'] ?? ''),
      endDate: String(raw['endDate'] ?? raw['EndDate'] ?? ''),
      status: String(raw['status'] ?? raw['Status'] ?? ''),
      createdAt: String(raw['createdAt'] ?? raw['CreatedAt'] ?? ''),
    }
  }
}
