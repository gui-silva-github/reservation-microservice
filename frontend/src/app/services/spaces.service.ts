import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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

    private authHeaders(): HttpHeaders {
        const token = localStorage.getItem('authToken');
        return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
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
                this.error.set('Não foi possível carregar os espaços. Verifique se o microserviço está ativo.');
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
            .post<unknown>(this.baseUrl, request, { headers: this.authHeaders() })
            .pipe(
                map((data: unknown) => this.toSpace(data as Record<string, unknown>)),
                tap(() => this.loadCatalog()),
            );
    }

    updateSpace(id: string, request: UpdateSpaceRequest): Observable<SpaceResponse> {
        return this.http
            .put<unknown>(`${this.baseUrl}/${id}`, request, { headers: this.authHeaders() })
            .pipe(
                map((data: unknown) => this.toSpace(data as Record<string, unknown>)),
                tap(() => this.loadCatalog()),
            );
    }

    deleteSpace(id: string): Observable<void> {
        return this.http
            .delete<void>(`${this.baseUrl}/${id}`, { headers: this.authHeaders() })
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