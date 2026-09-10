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
import { fadeSlide, listStagger } from '../../shared/animations/app.animations';
import { RevealDirective } from '../../shared/directives/reveal.directive';
@Component({
  selector: 'app-show-case',
  imports: [
    CurrencyPipe,
    MatCardModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule,
    RouterModule,
    RevealDirective,
  ],
  templateUrl: './show-case.html',
  styleUrl: './show-case.css',
  animations: [fadeSlide, listStagger],
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
