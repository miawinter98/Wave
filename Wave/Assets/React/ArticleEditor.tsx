import React, { useState, useEffect, useRef } from "react";
import { updateCharactersLeft, insertBeforeSelection, insertBeforeAndAfterSelection } from "../md_functions";

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

type ArticleView = {
	id: string,
	title: string,
	slug: string,
	markdown: string,
	status: number,
	publishDate: Date,
}

function get<T>(url: string): Promise<T> {
	return fetch(url, {
			method: "GET"
		}).then((response) => {
			if (!response.ok) throw new Error(response.statusText);
			
			return response.json() as Promise<T>;
		}).then(json => {
			return json as T;
		});
}

export default function Editor() {
	const [notice, setNotice] = useState<string>("");
	const [article, setArticle] = useState<ArticleView|null>(null);
	const target = document.getElementById("tool-target");

	useEffect(() => {
		get<ArticleView>("/api/article/68490edb-4cfb-40bf-badf-d9a803cd46d4")
			.then(result => {
				setNotice("");
				setArticle(result);
				console.log("Article loaded");
			})
			.catch(error => {
				setNotice(`Error loading Article: ${error.message}`);
				console.log(`Error loading Article: ${error.message}`);
				setArticle(null);
				return null;
			});
	}, ([setArticle, setNotice, console]) as any[]);

	const markdownArea = useRef(null);
	return (
		<>
				{
					notice.length < 1 ? 
						null : 
						<div role="alert" className="alert alert-error my-3">
							<p>{notice}</p>
						</div>
				}
				
				<div className="w-full">
				<ul className="steps steps-vertical md:steps-horizontal w-full lg:max-w-[40rem]">
					<li className={`step ${article?.status ?? -1 >= 0 ? "step-primary" : ""}`}>@Localizer["Draft"]</li>
					<li className={`step ${article?.status ?? -1 >= 1 ? "step-primary" : ""}`}>@Localizer["InReview"]</li>
					<li className={`step ${article?.status === 2 ? "step-primary" : ""}`}>@Localizer["Published"]</li>
				</ul>

				<form method="post">
					<div className="grid grid-cols-1 lg:grid-cols-2 gap-x-8">
						<label className="form-control w-full">
							<div className="label">
								@Localizer["Title_Label"]
							</div>
							<input className="input input-bordered w-full" defaultValue={article?.title}
								   maxLength={256} required aria-required autoComplete="off"
								   onInput={(event: FormEvent<HTMLInputElement>) => updateCharactersLeft(event.target)} placeholder='@Localizer["Title_Placeholder"]'>
							</input>
						</label>
						<label className="form-control w-full row-span-3 order-first md:order-none">
							<div className="label">
								@Localizer["Category_Label"]
							</div>
							<select className="select select-bordered w-full" size={10} multiple>
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
								   maxLength={64} autoComplete="off" defaultValue={article?.slug}
								   onInput={(event: FormEvent<HTMLInputElement>) => updateCharactersLeft(event.target)}
								   placeholder='@Localizer["Slug_Placeholder"]'
								   disabled={true} // TODO Article.Status is ArticleStatus.Published && Article.PublishDate < DateTimeOffset.UtcNow
								   >
							</input>
						</label>
						<label className="form-control w-full">
							<div className="label">
								@Localizer["PublishDate_Label"]
							</div>
							<input className="input input-bordered w-full" defaultValue={article?.publishDate}
								   type="datetime-local" autoComplete="off"
								   disabled={true} // TODO Article.Status is ArticleStatus.Published && Article.PublishDate < DateTimeOffset.UtcNow
								   >
							</input>
						</label>
					</div>
					
					<section className="my-6 grid grid-cols-1 lg:grid-cols-2 gap-x-8 gap-y-4">
						<div className="join join-vertical min-h-96 h-full w-full">
							<div className="flex flex-wrap gap-1 p-2 z-50 bg-base-200 sticky top-0" role="toolbar">
								<div className="join join-horizontal">
									<ToolBarButton title='@Localizer["Tools_H1_Tooltip"]' 
												   onClick={() => insertBeforeSelection(markdownArea.current, "# ", true)}>
										<strong>@Localizer["Tools_H1_Label"]</strong>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_H2_Tooltip"]' 
												   onClick={() => insertBeforeSelection(markdownArea.current, "## ", true)}>
										<strong>@Localizer["Tools_H2_Label"]</strong>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_H3_Tooltip"]' 
												   onClick={() => insertBeforeSelection(markdownArea.current, "### ", true)}>
										<strong>@Localizer["Tools_H3_Label"]</strong>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_H4_Tooltip"]' 
												   onClick={() => insertBeforeSelection(markdownArea.current, "#### ", true)}>
										<strong>@Localizer["Tools_H4_Label"]</strong>
									</ToolBarButton>
								</div>
								<div className="join join-horizontal">
									<ToolBarButton title='@Localizer["Tools_Bold_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "**")}>
										<strong>B</strong>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_Italic_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "*")}>
										<em>I</em>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_Underline_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "++")}>
										<span className="underline">U</span>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_StrikeThrough_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "~~")}>
										<del>@Localizer["Tools_StrikeThrough_Label"]</del>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_Mark_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "==")}>
										<mark>@Localizer["Tools_Mark_Label"]</mark>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_Mark_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "> ")}>
										| <em>@Localizer["Tools_Cite_Tooltip"]</em>
									</ToolBarButton>
								</div>
								<div className="join join-horizontal">
									<ToolBarButton onClick={() => insertBeforeSelection(markdownArea.current, "1. ", true)}>
										1.
									</ToolBarButton>
									<ToolBarButton onClick={() => insertBeforeSelection(markdownArea.current, "a. ", true)}>
										a.
									</ToolBarButton>
									<ToolBarButton onClick={() => insertBeforeSelection(markdownArea.current, "A. ", true)}>
										A.
									</ToolBarButton>
									<ToolBarButton onClick={() => insertBeforeSelection(markdownArea.current, "i. ", true)}>
										i.
									</ToolBarButton>
									<ToolBarButton onClick={() => insertBeforeSelection(markdownArea.current, "I. ", true)}>
										I.
									</ToolBarButton>
								</div>
								<div className="join join-horizontal">
									<ToolBarButton title='@Localizer["Tools_CodeLine_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "`")}>
										<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-4 h-4">
											<path fill-rule="evenodd" d="M14.447 3.026a.75.75 0 0 1 .527.921l-4.5 16.5a.75.75 0 0 1-1.448-.394l4.5-16.5a.75.75 0 0 1 .921-.527ZM16.72 6.22a.75.75 0 0 1 1.06 0l5.25 5.25a.75.75 0 0 1 0 1.06l-5.25 5.25a.75.75 0 1 1-1.06-1.06L21.44 12l-4.72-4.72a.75.75 0 0 1 0-1.06Zm-9.44 0a.75.75 0 0 1 0 1.06L2.56 12l4.72 4.72a.75.75 0 0 1-1.06 1.06L.97 12.53a.75.75 0 0 1 0-1.06l5.25-5.25a.75.75 0 0 1 1.06 0Z" clip-rule="evenodd" />
										</svg>
									</ToolBarButton>
									<ToolBarButton title='@Localizer["Tools_CodeBlock_Tooltip"]' 
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "```")}>
										<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-4 h-4">
											<path fill-rule="evenodd" d="M3 6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V6Zm14.25 6a.75.75 0 0 1-.22.53l-2.25 2.25a.75.75 0 1 1-1.06-1.06L15.44 12l-1.72-1.72a.75.75 0 1 1 1.06-1.06l2.25 2.25c.141.14.22.331.22.53Zm-10.28-.53a.75.75 0 0 0 0 1.06l2.25 2.25a.75.75 0 1 0 1.06-1.06L8.56 12l1.72-1.72a.75.75 0 1 0-1.06-1.06l-2.25 2.25Z" clip-rule="evenodd" />
										</svg>
									</ToolBarButton>
								</div>
							</div>
							<textarea ref={markdownArea} id="tool-target" className="resize-none textarea textarea-bordered outline-none w-full flex-1 join-item" 
									  required aria-required placeholder='@Localizer["Body_Placeholder"]'
									  autoComplete="off" defaultValue={article?.markdown}></textarea>
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
						
						<a className="btn w-full sm:btn-wide" href={`/article/${article?.id}`} // TODO disable when article not exists
							>
							@Localizer["ViewArticle_Label"]
						</a>
					</div>
				</form>
			</div>
		</>
	);
}


function ToolBarButton({title, onClick, children}: {title: string, onClick:MouseEventHandler<HTMLButtonElement>, children:any}) {
	return <button type="button" className="btn btn-accent btn-sm outline-none font-normal join-item" 
				   title={title}
				   onClick={onClick}>
			   {children ?? "err"}
		   </button>;
}