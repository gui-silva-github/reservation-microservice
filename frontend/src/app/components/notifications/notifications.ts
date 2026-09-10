import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { NotificationsService } from '../../services/notifications.service';
import { SpacesService } from '../../services/spaces.service';
import { UsersService } from '../../services/users.service';
import { NotificationResponse } from '../../models/notification-response';
import { getApiErrorMessage } from '../../utils/api-error.util';
import { fadeSlide, listStagger, unreadState } from '../../shared/animations/app.animations';
import { NotificationTypePipe } from '../../shared/pipes/notification-type.pipe';
import { RevealDirective } from '../../shared/directives/reveal.directive';
import { UnreadPulseDirective } from '../../shared/directives/unread-pulse.directive';

const GUID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

@Component({
  selector: 'app-notifications',
  imports: [
    DatePipe,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    NotificationTypePipe,
    RevealDirective,
    UnreadPulseDirective,
  ],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
  animations: [fadeSlide, listStagger, unreadState],
})
export class Notifications implements OnInit {
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);
  readonly notificationsService = inject(NotificationsService);
  readonly spacesService = inject(SpacesService);
  readonly usersService = inject(UsersService);

  readonly markingId = signal<string | null>(null);
  readonly markingAll = signal(false);
  readonly actionError = signal<string | null>(null);
  readonly selectedUserId = signal<string | null>(null);
  readonly adminLookupAttempted = signal(false);

  readonly adminLookupForm = this.formBuilder.nonNullable.group({
    userId: ['', [Validators.required, Validators.pattern(GUID_PATTERN)]],
  });

  get isAdminView(): boolean {
    return this.usersService.isAdmin;
  }

  ngOnInit(): void {
    if (!this.usersService.isAuthenticated) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    this.spacesService.ensureCatalog();

    if (this.isAdminView) {
      this.notificationsService.clear();
      return;
    }

    const userId = this.usersService.currentUserId;
    this.selectedUserId.set(userId);
    this.reloadNotifications();
  }

  readableMessage(notification: NotificationResponse): string {
    return this.spacesService.humanizeText(notification.message);
  }

  lookupAdminNotifications(): void {
    if (!this.isAdminView || this.adminLookupForm.invalid) {
      this.adminLookupForm.markAllAsTouched();
      return;
    }

    const userId = this.adminLookupForm.controls.userId.value.trim();
    this.selectedUserId.set(userId);
    this.adminLookupAttempted.set(true);
    this.actionError.set(null);
    this.notificationsService.loadByUser(userId);
  }

  reloadNotifications(): void {
    const userId = this.selectedUserId();
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
        const userId = this.selectedUserId();
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
    const userId = this.selectedUserId();
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
