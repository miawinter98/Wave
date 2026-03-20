import { inject, Injectable } from '@angular/core';
import type { paths, components } from '../models/schema';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

type ArticleResponse =
	paths['/api/article/latest']['get']['responses']['200']['content']['application/json'];

@Injectable({
	providedIn: 'root',
})
export class ArticleService {
	private client = inject(HttpClient);

	public getLatest(): Observable<ArticleResponse> {
		return this.client.get<ArticleResponse>(
			'http://localhost:5206/api/article/latest',
		);
	}
}
