import { Component, input } from '@angular/core';

@Component({
	selector: 'base-layout',
	imports: [],
	templateUrl: './base.layout.html',
})
export class BaseLayout {
	public title = input.required<string>();
}
