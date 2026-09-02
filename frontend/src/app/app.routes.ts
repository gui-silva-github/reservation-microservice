import { Routes } from '@angular/router';
import { Register } from './components/register/register';
import { Login } from './components/login/login';
import { ShowCase } from './components/show-case/show-case';
import { Search } from './components/search/search';
import { Products } from './components/products/products';
import { EditProduct } from './components/edit-product/edit-product';
import { NewProduct } from './components/new-product/new-product';
import { Orders } from './components/orders/orders';
import { BookingForm } from './components/booking-form/booking-form';
import { Notifications } from './components/notifications/notifications';

export const routes: Routes = [
    { path: '', redirectTo: 'spaces', pathMatch: 'full' },
    { path: 'auth/register', component: Register },
    { path: 'auth/login', component: Login },
    { path: 'spaces', component: ShowCase },
    { path: 'spaces/search/:str', component: Search },
    { path: 'spaces/search', component: Search },
    { path: 'spaces/:id/book', component: BookingForm },
    { path: 'bookings', component: Orders },
    { path: 'notifications', component: Notifications },
    { path: 'admin/spaces', component: Products },
    { path: 'admin/spaces/create', component: NewProduct },
    { path: 'admin/spaces/edit/:id', component: EditProduct },
    { path: '**', redirectTo: 'spaces' },
];