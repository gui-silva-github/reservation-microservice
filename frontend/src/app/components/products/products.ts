import { Component, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { SpaceResponse } from '../../models/space-response';
import { SpacesService } from '../../services/spaces.service';
import { UsersService } from '../../services/users.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { DeleteProductDialog } from '../delete-product-dialog/delete-product-dialog';
import { RevealDirective } from '../../shared/directives/reveal.directive';
import { fadeSlide } from '../../shared/animations/app.animations';

@Component({
  selector: 'app-products',
  imports: [
    CurrencyPipe,
    MatCardModule,
    MatButtonModule,
    RouterModule,
    MatTableModule,
    MatIconModule,
    RevealDirective,
  ],
  templateUrl: './products.html',
  styleUrl: './products.css',
  animations: [fadeSlide],
})
export class Products {
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  readonly spacesService = inject(SpacesService);
  readonly usersService = inject(UsersService);

  displayedColumns: string[] = ['name', 'location', 'capacity', 'pricePerHour', 'isActive', 'actions'];

  constructor() {
    if (!this.usersService.isAuthenticated || !this.usersService.isAdmin) {
      this.router.navigate(['/auth', 'login']);
      return;
    }

    if (!this.spacesService.hasSpaces()) {
      this.spacesService.loadCatalog();
    }
  }

  edit(space: SpaceResponse): void {
    this.router.navigate(['/admin', 'spaces', 'edit', space.id]);
  }

  delete(space: SpaceResponse): void {
    this.dialog.open(DeleteProductDialog, {
      width: '460px',
      maxWidth: '95vw',
      panelClass: ['app-dialog', 'app-dialog--danger'],
      data: { space },
      autoFocus: false,
    });
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'stock-badge stock-badge--ok' : 'stock-badge stock-badge--out';
  }

  getStatusLabel(isActive: boolean): string {
    return isActive ? 'Ativo' : 'Inativo';
  }

  getStatusIcon(isActive: boolean): string {
    return isActive ? 'check_circle' : 'error_outline';
  }
}
