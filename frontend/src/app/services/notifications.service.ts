import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environment';
import { NotificationResponse } from '../models/notification-response';

@Injectable({
    providedIn: 'root',
})
export class NotificationsService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = environment.notificationsUrl.replace(/\/$/, '');

    readonly notifications = signal<NotificationResponse[]>([]);
    readonly loading = signal(false);
    readonly error = signal<string | null>(null);
    readonly hasNotifications = computed(() => this.notifications().length > 0);
    readonly unreadCount = computed(() => this.notifications().filter((notification: NotificationResponse) => !notification.read).length)

    private authHeaders(): HttpHeaders {
        const token = localStorage.getItem('authToken');
        return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
    }

    getByUserId(userId: string): Observable<NotificationResponse[]> {
        return this.http
            .get<unknown>(`${this.baseUrl}/userid/${userId}`, { headers: this.authHeaders()})
            .pipe(map((data: unknown) => this.toNotifications(data)));
    }

    loadByUser(userId: string): void {
        this.loading.set(true);
        this.error.set(null);

        this.getByUserId(userId).subscribe({
            next: (notifications: NotificationResponse[]) => {
                this.notifications.set(this.sortByDateDesc(notifications));
                this.loading.set(false);
            },
            error: () => {
                this.notifications.set([]);
                this.error.set(
                    'Não foi possível carregar as notificações. Verifique se o microserviço está ativo.',
                );
                this.loading.set(false);
            }
        });
    }

    private sortByDateDesc(notifications: NotificationResponse[]): NotificationResponse[] {
        return [...notifications].sort(
            (first, second) => new Date(second.sentAt).getTime() - new Date(first.sentAt).getTime(),
        )
    }

    private toNotifications(data: unknown): NotificationResponse[] {
        if (!data) {
            return [];
        }

        if (!Array.isArray(data)) {
            return [this.toNotification(data as Record<string, unknown>)];
        }

        return data
            .filter((item) => item != null)
            .map((item) => this.toNotification(item as Record<string, unknown>));
    }

    private toNotification(raw: Record<string, unknown>): NotificationResponse {
        return {
            id: String(raw['id'] ?? raw['Id'] ?? ''),
            userId: String(raw['userId'] ?? raw['UserId'] ?? ''),
            message: String(raw['message'] ?? raw['Message'] ?? ''),
            type: String(raw['type'] ?? raw['Type'] ?? ''),
            sentAt: String(raw['sentAt'] ?? raw['SentAt'] ?? ''),
            read: Boolean(raw['read'] ?? raw['Read'] ?? false),
        };
    }
}