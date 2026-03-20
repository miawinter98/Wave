import { Component, inject } from '@angular/core';
import { ArticleService } from '../../services/article.service';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
	selector: 'app-article',
	imports: [],
	templateUrl: './article.view.html',
})
export class ArticleView {
	private service = inject(ArticleService);
	protected latestArticle = toSignal(this.service.getLatest());
}
