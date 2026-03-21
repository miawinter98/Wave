import { inject, Injectable } from '@angular/core';
import type { paths, components } from '../models/schema';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export type ArticleResponse =
	paths['/api/article/latest']['get']['responses']['200']['content']['application/json'];

@Injectable({
	providedIn: 'root',
})
export class ArticleService {
	private client = inject(HttpClient);

	public getLatest(): Observable<ArticleResponse> {
		return this.client.get<ArticleResponse>(
			new URL('/api/article/latest', environment.apiUrl).toString(),
		);
	}

	public getById(id: string): Observable<ArticleResponse | undefined> {
		return this.client.get<ArticleResponse | undefined>(
			new URL(`/api/article/${id}`, environment.apiUrl).toString(),
		);
	}
}
