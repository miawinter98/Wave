import {
	ChangeDetectionStrategy,
	Component,
	inject,
	input,
} from '@angular/core';
import { ArticleService } from '../../services/article.service';
import { BaseLayout } from '../../layouts/base/base.layout';
import sanitizeHtml from 'sanitize-html';
import { marked } from 'marked';
import { DatePipe } from '@angular/common';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';

@Component({
	selector: 'app-article',
	imports: [BaseLayout, DatePipe],
	templateUrl: './article.view.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleView {
	private service = inject(ArticleService);
	protected article = rxResource({
		params: () => ({ id: this.id() }),
		stream: ({ params }) => this.service.getById(params.id ?? ''),
	});

	public id = input<string>();

	protected parseBodySafe(body: string): string {
		return sanitizeHtml(marked.parse(body, { async: false }));
	}
}
