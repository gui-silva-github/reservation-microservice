import {
  animate,
  group,
  query,
  stagger,
  state,
  style,
  transition,
  trigger,
} from '@angular/animations';

export const routeFadeAnimation = trigger('routeFade', [
  transition('* <=> *', [
    group([
      query(':leave', [
        style({ opacity: 1, transform: 'none', pointerEvents: 'none' }),
        animate('120ms ease-out', style({ opacity: 0, transform: 'none' })),
      ], { optional: true }),
      query(':enter', [
        style({ opacity: 0, transform: 'none' }),
        animate('180ms ease-out', style({ opacity: 1, transform: 'none' })),
      ], { optional: true }),
    ]),
  ]),
]);

export const fadeSlide = trigger('fadeSlide', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('220ms ease-out', style({ opacity: 1 })),
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
        style({ opacity: 0 }),
        stagger(40, [
          animate(
            '280ms ease-out',
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
    style({ opacity: 0, transform: 'translateY(4px)' }),
    animate('240ms ease-out', style({ opacity: 1, transform: 'none' })),
  ]),
  transition(':leave', [
    animate('140ms ease-out', style({ opacity: 0 })),
  ]),
]);

export const unreadState = trigger('unreadState', [
  state('unread', style({ borderLeftWidth: '4px' })),
  state('read', style({ borderLeftWidth: '0' })),
  transition('unread <=> read', animate('220ms ease')),
]);
