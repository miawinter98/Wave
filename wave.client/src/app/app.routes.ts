import { Routes } from '@angular/router';
import { ArticleView } from './views/article/article.view';

export const routes: Routes = [
	{
		path: 'article/:id',
		component: ArticleView,
	},
];
