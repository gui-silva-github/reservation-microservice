import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnimationEvent } from '@angular/animations';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLinkActive, RouterModule, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from '@angular/material/toolbar';
import { UsersService } from './services/users.service';
import { routeFadeAnimation } from './shared/animations/app.animations';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule, RouterOutlet, MatButtonModule, MatIconModule, MatInputModule,
    MatFormFieldModule, MatToolbarModule, RouterModule, RouterLinkActive, ReactiveFormsModule,
  ],
  templateUrl: './app.html',
  styleUrl: './app.css',
  animations: [routeFadeAnimation],
})
export class App {
  readonly usersService = inject(UsersService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  readonly routeReady = signal(true);

  searchForm: FormGroup = this.fb.group({
    searchStr: ['', []],
  });

  prepareRoute(outlet: RouterOutlet): string {
    return outlet?.isActivated ? (outlet.activatedRoute.snapshot.routeConfig?.path ?? '') : '';
  }

  onRouteAnimationStart(): void {
    this.routeReady.set(false);
  }

  onRouteAnimationDone(_event: AnimationEvent): void {
    this.routeReady.set(true);
  }

  logout() {
    this.usersService.logout();
    this.router.navigate(['/spaces']);
  }

  search() {
    const term = (this.searchForm.value.searchStr ?? '').trim();

    if (!term) {
      this.router.navigate(['/spaces']);
      return;
    }

    this.router.navigate(['/spaces', 'search', term]);
  }
}
