import { useState } from "react";

/*
 TODO: load categories
<InputSelect class="select select-bordered w-full" @bind-Value="@Model.Categories" multiple size="10">
	@foreach (var group in Categories.GroupBy(c => c.Color)) {
		<optgroup class="font-bold not-italic my-3" label="@group.Key.Humanize()">
			@foreach (var category in group) {
				<option value="@category.Id" selected="@Model.Categories?.Contains(category.Id)">@category.Name</option>
			}
		</optgroup>
	}
</InputSelect>
 */

export default function Editor() {
	const status = "draft";

	return (
		<>
				<div className="w-full">
				<ul className="steps steps-vertical md:steps-horizontal w-full lg:max-w-[40rem]">
					<li className={`step ${status === "draft" ? "step-primary" : ""}`}>@Localizer["Draft"]</li>
					<li className={`step ${status === "in_review" ? "step-primary" : ""}`}>@Localizer["InReview"]</li>
					<li className={`step ${status === "published" ? "step-primary" : ""}`}>@Localizer["Published"]</li>
				</ul>

				<form method="post">
					<div className="grid grid-cols-1 lg:grid-cols-2 gap-x-8">
						<label className="form-control w-full">
							<div className="label">
								@Localizer["Title_Label"]
							</div>
							<input className="input input-bordered w-full"
							       maxlength="256" required aria-required autocomplete="off"
							       oninput="charactersLeft_onInput(this)" placeholder='@Localizer["Title_Placeholder"]'>
							</input>
						</label>
						<label className="form-control w-full row-span-3 order-first md:order-none">
							<div className="label">
								@Localizer["Category_Label"]
							</div>
							<select className="select select-bordered w-full" size="10" multiple>
								<optgroup className="font-bold not-italic my-3" label="TODO">
									<option>todo</option>
								</optgroup>
							</select>
						</label>
						<label className="form-control w-full">
							<div className="label">
								@Localizer["Slug_Label"]
							</div>
							<input className="input input-bordered w-full"
							       maxlength="64" autocomplete="off"
							       oninput="charactersLeft_onInput(this)" placeholder='@Localizer["Slug_Placeholder"]'
							       disabled={true} // TODO Article.Status is ArticleStatus.Published && Article.PublishDate < DateTimeOffset.UtcNow
							       >
							</input>
						</label>
						<label className="form-control w-full">
							<div className="label">
								@Localizer["PublishDate_Label"]
							</div>
							<input className="input input-bordered w-full"
							       type="datetime-local" autocomplete="off"
							       disabled={true} // TODO Article.Status is ArticleStatus.Published && Article.PublishDate < DateTimeOffset.UtcNow
							       >
							</input>
						</label>
					</div>
					
					<section className="my-6 grid grid-cols-1 lg:grid-cols-2 gap-x-8 gap-y-4">
						<div className="join join-vertical min-h-96 h-full w-full">
							<div className="flex flex-wrap gap-1 p-2 z-50 bg-base-200 sticky top-0" aria-role="toolbar">
								<div className="join join-horizontal">
									<button type="button" className="btn btn-accent btn-sm outline-none font-normal join-item" 
									        title='@Localizer["Tools_H1_Tooltip"]'
									        onclick="window.insertBeforeSelection('# ', true);">
										<strong>@Localizer["Tools_H1_Label"]</strong>
									</button>
								</div>
								<div className="join join-horizontal">
									
								</div>
								<div className="join join-horizontal">
									
								</div>
							</div>
							<textarea id="tool-target" className="resize-none textarea textarea-bordered outline-none w-full flex-1 join-item" 
							          required aria-required placeholder='@Localizer["Body_Placeholder"]'
							          autocomplete="off"></textarea>
						</div>
						<div className="bg-base-200 p-2">
							<h2 className="text-2xl lg:text-4xl font-bold mb-6 hyphens-auto">@Title</h2>
							<div className="prose prose-neutral max-w-none hyphens-auto text-justify">
								<h2>Hello World!</h2>
								<p>Html Preview Here</p>
							</div>
							<div className="flex flex-col gap-4">
								<div className="skeleton h-4 w-full"></div>
								<div className="skeleton h-4 w-full"></div>
								<div className="skeleton h-32 w-full"></div>
								<div className="skeleton h-4 w-full"></div>
								<div className="skeleton h-4 w-full"></div>
								<div className="skeleton h-4 w-full"></div>
							</div>
						</div>
					</section>

					<div className="flex gap-2 flex-wrap mt-3">
						<button type="submit" className="btn btn-primary w-full sm:btn-wide" disabled={true} // TODO implement when saving
							>
							@Localizer["EditorSubmit"]
						</button>
						
						<a className="btn w-full sm:btn-wide" href="/article/{Article.Id" // TODO disable when article not exists
							>
							@Localizer["ViewArticle_Label"]
						</a>
					</div>
				</form>
			</div>
		</>
	);
}