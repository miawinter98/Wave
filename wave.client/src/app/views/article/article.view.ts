import { Component, inject } from '@angular/core';
import { ArticleService } from '../../services/article.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { BaseLayout } from '../../layouts/base/base.layout';
import sanitizeHtml from 'sanitize-html';
import { marked } from 'marked';
import { DatePipe } from '@angular/common';

@Component({
	selector: 'app-article',
	imports: [BaseLayout, DatePipe],
	templateUrl: './article.view.html',
})
export class ArticleView {
	private service = inject(ArticleService);
	protected latestArticle = toSignal(this.service.getLatest());

	protected parseBodySafe(body: string): string {
		return sanitizeHtml(marked.parse(body, { async: false }));
	}
}
