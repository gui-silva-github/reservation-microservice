import {
  animate,
  query,
  stagger,
  state,
  style,
  transition,
  trigger,
} from '@angular/animations';

/** Soft page content enter — short fade, no big vertical travel. */
export const pageEnter = trigger('pageEnter', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(10px)' }),
    animate(
      '380ms cubic-bezier(0.22, 1, 0.36, 1)',
      style({ opacity: 1, transform: 'none' }),
    ),
  ]),
]);

export const fadeSlide = trigger('fadeSlide', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(8px)' }),
    animate(
      '280ms cubic-bezier(0.22, 1, 0.36, 1)',
      style({ opacity: 1, transform: 'none' }),
    ),
  ]),
  transition(':leave', [
    animate('140ms ease-out', style({ opacity: 0 })),
  ]),
]);

export const listStagger = trigger('listStagger', [
  transition(':enter', [
    query(
      '.product, .order-card, .notification-card',
      [
        style({ opacity: 0, transform: 'translateY(14px) scale(0.98)' }),
        stagger(55, [
          animate(
            '360ms cubic-bezier(0.22, 1, 0.36, 1)',
            style({ opacity: 1, transform: 'none' }),
          ),
        ]),
      ],
      { optional: true },
    ),
  ]),
]);

export const cardEnter = trigger('cardEnter', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(10px)' }),
    animate(
      '300ms cubic-bezier(0.22, 1, 0.36, 1)',
      style({ opacity: 1, transform: 'none' }),
    ),
  ]),
  transition(':leave', [
    animate('140ms ease-out', style({ opacity: 0, transform: 'scale(0.98)' })),
  ]),
]);

export const unreadState = trigger('unreadState', [
  state('unread', style({ borderLeftWidth: '3px' })),
  state('read', style({ borderLeftWidth: '0' })),
  transition('unread <=> read', animate('220ms ease')),
]);
