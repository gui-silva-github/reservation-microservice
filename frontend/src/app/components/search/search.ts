import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { SpacesService } from '../../services/spaces.service';
import { SpaceResponse } from '../../models/space-response';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { UsersService } from '../../services/users.service';
import { fadeSlide, listStagger } from '../../shared/animations/app.animations';
import { RevealDirective } from '../../shared/directives/reveal.directive';
@Component({
  selector: 'app-search',
  imports: [
    CurrencyPipe,
    MatCardModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule,
    RouterModule,
    RevealDirective,
  ],
  templateUrl: './search.html',
  styleUrl: './search.css',
  animations: [fadeSlide, listStagger],
})
export class Search implements OnInit {
  private readonly spacesService = inject(SpacesService);
  private readonly activatedRoute = inject(ActivatedRoute);
  readonly usersService = inject(UsersService);

  readonly spaces = signal<SpaceResponse[]>([]);
  readonly searchTerm = signal('');
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly hasResults = () => this.spaces().length > 0;

  ngOnInit(): void {
    this.activatedRoute.paramMap.subscribe((params) => {
      const term = params.get('str') ?? '';
      this.searchTerm.set(term);
      this.loading.set(true);
      this.error.set(null);

      this.spacesService.searchSpaces(term).subscribe({
        next: (response) => {
          this.spaces.set(response);
          this.loading.set(false);
        },
        error: () => {
          this.spaces.set([]);
          this.loading.set(false);
          this.error.set('Não foi possível realizar a busca. Verifique se o serviço está ativo.');
        },
      });
    });
  }

  canBook(space: SpaceResponse): boolean {
    return space.isActive;
  }
}