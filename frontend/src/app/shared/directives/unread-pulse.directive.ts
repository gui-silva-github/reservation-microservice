import { Directive, input } from '@angular/core';

@Directive({
  selector: '[appUnreadPulse]',
  host: {
    '[class.unread-pulse]': 'unread()',
  },
})
export class UnreadPulseDirective {
  readonly unread = input(false, { alias: 'appUnreadPulse' });
}
