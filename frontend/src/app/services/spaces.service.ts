import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../environment';
import { SpaceResponse } from '../models/space-response';
import { CreateSpaceRequest } from '../models/create-space-request';
import { UpdateSpaceRequest } from '../models/update-space-request';

@Injectable({
    providedIn: 'root',
})
export class SpacesService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = environment.spacesUrl.replace(/\/$/, '');

    readonly catalog = signal<SpaceResponse[]>([]);
    readonly loading = signal(false);
    readonly error = signal<string | null>(null);
    readonly hasSpaces = computed(() => this.catalog().length > 0);

    ensureCatalog(): void {
        if (this.catalog().length > 0 || this.loading()) {
            return;
        }

        this.http.get<unknown>(this.baseUrl).subscribe({
            next: (data: unknown) => this.catalog.set(this.toSpaces(data)),
            error: () => undefined,
        });
    }

    nameById(spaceId: string): string {
        return this.findById(spaceId)?.name || 'Espaço';
    }

    locationById(spaceId: string): string {
        return this.findById(spaceId)?.location || '';
    }

    private findById(spaceId: string): SpaceResponse | undefined {
        const normalizedId = spaceId.toLowerCase();
        return this.catalog().find((item) => item.id.toLowerCase() === normalizedId);
    }

    humanizeText(text: string): string {
        let result = text ?? '';

        for (const space of this.catalog()) {
            result = result.replace(new RegExp(space.id, 'gi'), space.name);
        }

        result = result.replace(/Sua reserva\s+[0-9a-f-]{36}\s+para o espaço/gi, 'Sua reserva no espaço');
        result = result.replace(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi, '');
        return result.replace(/\s+/g, ' ').replace(/\s+([.,;:])/g, '$1').trim();
    }

    loadCatalog(): void {
        this.loading.set(true);
        this.error.set(null);

        this.http.get<unknown>(this.baseUrl).subscribe({
            next: (data: unknown) => {
                this.catalog.set(this.toSpaces(data));
                this.loading.set(false);
            },
            error: () => {
                this.catalog.set([]);
                this.error.set('Não foi possível carregar os espaços. Verifique se o serviço está ativo.');
                this.loading.set(false);
            },
        });
    }

    searchSpaces(searchString: string): Observable<SpaceResponse[]> {
        const term = encodeURIComponent(searchString ?? '');
        return this.http
            .get<unknown>(`${this.baseUrl}/search/${term}`)
            .pipe(map((data: unknown) => this.toSpaces(data)));
    }

    getSpaceById(id: string): Observable<SpaceResponse> {
        return this.http
            .get<unknown>(`${this.baseUrl}/${id}`)
            .pipe(map((data: unknown) => this.toSpace(data as Record<string, unknown>)));
    }

    createSpace(request: CreateSpaceRequest): Observable<SpaceResponse> {
        return this.http
            .post<unknown>(this.baseUrl, request)
            .pipe(
                map((data: unknown) => this.toSpace(data as Record<string, unknown>)),
                tap(() => this.loadCatalog()),
            );
    }

    updateSpace(id: string, request: UpdateSpaceRequest): Observable<SpaceResponse> {
        return this.http
            .put<unknown>(`${this.baseUrl}/${id}`, request)
            .pipe(
                map((data: unknown) => this.toSpace(data as Record<string, unknown>)),
                tap(() => this.loadCatalog()),
            );
    }

    deleteSpace(id: string): Observable<void> {
        return this.http
            .delete<void>(`${this.baseUrl}/${id}`)
            .pipe(tap(() => this.catalog.update((items: SpaceResponse[]) => items.filter((s) => s.id !== id))));
    }

    private toSpaces(data: unknown): SpaceResponse[] {
        if (!data) {
            return [];
        }

        if (!Array.isArray(data)) {
            return [this.toSpace(data as Record<string, unknown>)];
        }

        return data
            .filter((item) => item != null)
            .map((item) => this.toSpace(item as Record<string, unknown>));
    }

    private toSpace(raw: Record<string, unknown>): SpaceResponse {
        return {
            id: String(raw['id'] ?? raw['Id'] ?? ''),
            name: String(raw['name'] ?? raw['Name'] ?? ''),
            location: String(raw['location'] ?? raw['Location'] ?? ''),
            description: (raw['description'] ?? raw['Description'] ?? null) as string | null,
            capacity: Number(raw['capacity'] ?? raw['Capacity'] ?? 0),
            pricePerHour: Number(raw['pricePerHour'] ?? raw['PricePerHour'] ?? 0),
            isActive: Boolean(raw['isActive'] ?? raw['IsActive'] ?? true),
        };
    }
}