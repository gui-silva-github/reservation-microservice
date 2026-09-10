import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'statusLabel',
})
export class StatusLabelPipe implements PipeTransform {
  transform(status: string | null | undefined): string {
    switch ((status ?? '').toLowerCase()) {
      case 'confirmed':
        return 'Confirmada';
      case 'cancelled':
        return 'Cancelada';
      case 'pending':
        return 'Pendente';
      default:
        return status || 'Indefinido';
    }
  }
}

@Pipe({
  name: 'statusClass',
})
export class StatusClassPipe implements PipeTransform {
  transform(status: string | null | undefined): string {
    switch ((status ?? '').toLowerCase()) {
      case 'confirmed':
        return 'stock-badge stock-badge--ok';
      case 'cancelled':
        return 'stock-badge stock-badge--out';
      case 'pending':
        return 'stock-badge stock-badge--low';
      default:
        return 'stock-badge';
    }
  }
}
