import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { NotificationsService } from '../../services/notifications.service';
import { SpacesService } from '../../services/spaces.service';
import { UsersService } from '../../services/users.service';
import { NotificationResponse } from '../../models/notification-response';
import { getApiErrorMessage } from '../../utils/api-error.util';
import { fadeSlide, listStagger, unreadState } from '../../shared/animations/app.animations';
import { NotificationTypePipe } from '../../shared/pipes/notification-type.pipe';
import { RevealDirective } from '../../shared/directives/reveal.directive';
import { UnreadPulseDirective } from '../../shared/directives/unread-pulse.directive';

@Component({
  selector: 'app-notifications',
  imports: [
    DatePipe, RouterModule, MatButtonModule, MatCardModule,
    MatIconModule, NotificationTypePipe, RevealDirective, UnreadPulseDirective,
  ],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
  animations: [fadeSlide, listStagger, unreadState],
})
export class Notifications implements OnInit {
  private readonly router = inject(Router);
  readonly notificationsService = inject(NotificationsService);
  readonly spacesService = inject(SpacesService);
  readonly usersService = inject(UsersService);

  readonly markingId = signal<string | null>(null);
  readonly markingAll = signal(false);
  readonly actionError = signal<string | null>(null);

  ngOnInit(): void {
    if (!this.usersService.isAuthenticated || this.usersService.isAdmin) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    this.spacesService.ensureCatalog();
    this.reloadNotifications();
  }

  readableMessage(notification: NotificationResponse): string {
    return this.spacesService.humanizeText(notification.message);
  }

  reloadNotifications(): void {
    const userId = this.usersService.currentUserId;
    if (userId) {
      this.notificationsService.loadByUser(userId);
    }
  }

  markAsRead(notification: NotificationResponse): void {
    if (notification.read || this.markingId()) {
      return;
    }

    this.markingId.set(notification.id);
    this.actionError.set(null);

    this.notificationsService.markAsRead(notification.id).subscribe({
      next: () => {
        this.notificationsService.markLocalAsRead(notification.id);
        const userId = this.usersService.currentUserId;
        if (userId) {
          this.notificationsService.invalidateByUser(userId);
        }
        this.markingId.set(null);
      },
      error: (error) => {
        this.markingId.set(null);
        this.actionError.set(getApiErrorMessage(error, 'Não foi possível marcar a notificação como lida.'));
      },
    });
  }

  markAllAsRead(): void {
    const userId = this.usersService.currentUserId;
    if (!userId || this.markingAll() || this.notificationsService.unreadCount() === 0) {
      return;
    }

    this.markingAll.set(true);
    this.actionError.set(null);

    this.notificationsService.markAllAsRead(userId).subscribe({
      next: () => {
        this.notificationsService.markLocalAllAsRead();
        this.notificationsService.invalidateByUser(userId);
        this.markingAll.set(false);
      },
      error: (error) => {
        this.markingAll.set(false);
        this.actionError.set(getApiErrorMessage(error, 'Não foi possível marcar as notificações como lidas.'));
      },
    });
  }

}
