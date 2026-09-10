import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'spaces', pathMatch: 'full' },
  {
    path: 'auth/register',
    loadComponent: () => import('./components/register/register').then((m) => m.Register),
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./components/login/login').then((m) => m.Login),
  },
  {
    path: 'spaces',
    loadComponent: () => import('./components/show-case/show-case').then((m) => m.ShowCase),
  },
  {
    path: 'spaces/search/:str',
    loadComponent: () => import('./components/search/search').then((m) => m.Search),
  },
  {
    path: 'spaces/search',
    loadComponent: () => import('./components/search/search').then((m) => m.Search),
  },
  {
    path: 'spaces/:id/book',
    loadComponent: () => import('./components/booking-form/booking-form').then((m) => m.BookingForm),
  },
  {
    path: 'bookings',
    loadComponent: () => import('./components/orders/orders').then((m) => m.Orders),
  },
  {
    path: 'notifications',
    loadComponent: () => import('./components/notifications/notifications').then((m) => m.Notifications),
  },
  {
    path: 'admin/spaces',
    loadComponent: () => import('./components/products/products').then((m) => m.Products),
  },
  {
    path: 'admin/spaces/create',
    loadComponent: () => import('./components/new-product/new-product').then((m) => m.NewProduct),
  },
  {
    path: 'admin/spaces/edit/:id',
    loadComponent: () => import('./components/edit-product/edit-product').then((m) => m.EditProduct),
  },
  { path: '**', redirectTo: 'spaces' },
];
