import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
	{
		path: 'article',
		renderMode: RenderMode.Server,
	},
	{
		path: '**',
		renderMode: RenderMode.Prerender,
	},
];
