import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { NotificationsService } from '../../services/notifications.service';
import { UsersService } from '../../services/users.service';
import { NotificationResponse } from '../../models/notification-response';

@Component({
  selector: 'app-notifications',
  imports: [
    DatePipe, RouterModule, MatButtonModule, MatCardModule,
    MatIconModule
  ],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
})
export class Notifications implements OnInit {
  private readonly router = inject(Router);
  readonly notificationsService = inject(NotificationsService);
  readonly usersService = inject(UsersService);

  ngOnInit(): void {
    if (!this.usersService.isAuthenticated || this.usersService.isAdmin) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    this.reloadNotifications();
  }

  reloadNotifications(): void {
    const userId = this.usersService.currentUserId;
    if (userId) {
      this.notificationsService.loadByUser(userId);
    }
  }

  typeLabel(notification: NotificationResponse): string {
    switch (notification.type.toLowerCase()) {
      case 'bookingcreated':
        return 'Reserva criada';
      case 'bookingcancelled':
        return 'Reserva cancelada';
      default:
        return notification.type || 'Aviso';
    }
  }

  typeIcon(notification: NotificationResponse): string {
    switch (notification.type.toLowerCase()) {
      case 'bookingcreated':
        return 'event_available';
      case 'bookingcancelled':
        return 'event_busy';
      default:
        return 'notifications';
    }
  }
}
