import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'notificationType',
})
export class NotificationTypePipe implements PipeTransform {
  transform(type: string | null | undefined, kind: 'label' | 'icon' = 'label'): string {
    const normalized = (type ?? '').toLowerCase();

    if (kind === 'icon') {
      switch (normalized) {
        case 'bookingcreated':
          return 'event_available';
        case 'bookingcancelled':
          return 'event_busy';
        default:
          return 'notifications';
      }
    }

    switch (normalized) {
      case 'bookingcreated':
        return 'Reserva criada';
      case 'bookingcancelled':
        return 'Reserva cancelada';
      default:
        return type || 'Aviso';
    }
  }
}
