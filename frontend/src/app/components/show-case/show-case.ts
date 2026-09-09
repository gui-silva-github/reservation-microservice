import { Component, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { SpacesService } from '../../services/spaces.service';
import { SpaceResponse } from '../../models/space-response';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-show-case',
  imports: [
    CurrencyPipe,
    MatCardModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule,
    RouterModule,
  ],
  templateUrl: './show-case.html',
  styleUrl: './show-case.css',
})
export class ShowCase {
  readonly spacesService = inject(SpacesService);
  readonly usersService = inject(UsersService);

  constructor() {
    if (!this.spacesService.hasSpaces()) {
      this.spacesService.loadCatalog();
    }
  }
  
  canBook(space: SpaceResponse): boolean {
    return space.isActive;
  }
}
