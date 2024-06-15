import React, { useState, useEffect, useRef } from "react";
import { updateCharactersLeft, insertBeforeSelection, insertBeforeAndAfterSelection } from "../utilities/md_functions";
import { LabelInput, ToolBarButton } from "./Forms";
import { useTranslation, Trans } from 'react-i18next';
import "groupby-polyfill/lib/polyfill.js";

enum CategoryColor {
	Primary = 1, 
	Dangerous = 5, 
	Important = 10, 
	Informative = 15, 
	Secondary = 20,
	Default = 25, 
	Extra = 50, 
}

type Category = {
	id: string,
	name: string,
	color: CategoryColor,
}

type ArticleView = {
	id: string,
	title: string,
	slug: string,
	body: string,
	status: number,
	publishDate: Date,
	categories: Category[],
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
	const { t } = useTranslation();
	const [notice, setNotice] = useState<string>("");
	const [article, setArticle] = useState<ArticleView|null>(null);
	const [categories, setCategories] = useState<Category[]>([]);

	const location = window.location.pathname;
	useEffect(() => {
		if (categories.length < 1) {
			get<Category[]>("/api/categories?all=true").then(result => {
				setCategories(result);
			}).catch(error => {
				setNotice(`Error loading Categories: ${error.Message}`);
				console.log(`Error loading Categories: ${error.message}`);
			});
		}

		const id = location.match(/article\/([0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12})\/edit/i);
		if (!id) {
			const publishDate = new Date();
			publishDate.setDate(publishDate.getDate() + 7);
			const article : ArticleView = {
				body: "",
				id: "",
				publishDate: publishDate,
				slug: "",
				status: 0,
				title: ""};
			setArticle(article);
		} else if (!article) {
			get<ArticleView>(`/api/article/${id[1]}`)
				.then(result => {
					setNotice("");
					setArticle(result);
					console.log("Article loaded");
				})
				.catch(error => {
					setNotice(`Error loading Article: ${error.message}`);
					console.log(`Error loading Article: ${error.message}`);
					setArticle(null);
				});
		}
	}, ([setArticle, setNotice, console, location]) as any[]);

	const markdownArea = useRef<HTMLTextAreaElement>(null);
	return (
		<>
				{
					notice.length > 0 &&
						<div role="alert" className="alert alert-error my-3">
							<p>{notice}</p>
						</div>
				}
				
				<div className="w-full">
				<ul className="steps steps-vertical md:steps-horizontal w-full lg:max-w-[40rem]">
					<li className={`step ${article?.status ?? -1 >= 0 ? "step-primary" : ""}`}>{t("Draft")}</li>
					<li className={`step ${article?.status ?? -1 >= 1 ? "step-primary" : ""}`}>{t("InReview")}</li>
					<li className={`step ${article?.status === 2 ? "step-primary" : ""}`}>{t("Published")}</li>
				</ul>

				<form method="post">
					<fieldset className="grid grid-cols-1 lg:grid-cols-2 gap-x-8" disabled={!article}>
						<LabelInput label={t("Title_Label")}>
							<input className="input input-bordered w-full" defaultValue={article?.title}
								   maxLength={256} required aria-required autoComplete="off"
								   onInput={(event: React.FormEvent<HTMLInputElement>) => updateCharactersLeft(event.target as HTMLInputElement)} 
								   placeholder={t("Title_Placeholder")}>
							</input>
						</LabelInput>
						<LabelInput label={t("Category_Label")} className="row-span-3 order-first md:order-none">
							<select className="select select-bordered w-full" size={10} multiple>
								{
									categories.length < 1 && <option disabled>loading...</option>
								}
								{
									Array.from(Map.groupBy(categories, (c: Category) => c.color) as Map<CategoryColor, Category[]>)
										.map((value, _) => 
											<optgroup className="font-bold not-italic my-3" label={t(`Category.${CategoryColor[value[0]]}`)}>
												{value[1].map(c =>
													<option key={c.id} value={c.id} selected={(article?.categories?.findIndex(c1 => c1.id == c.id) ?? -1) > -1}>
														{c.name ?? "err"}</option>
												)}
											</optgroup>)
								}
							</select>
						</LabelInput>
						<LabelInput label={t("Slug_Label")}>
							<input className="input input-bordered w-full"
							       maxLength={64} autoComplete="off" defaultValue={article?.slug}
							       onInput={(event: React.FormEvent<HTMLInputElement>) => updateCharactersLeft(event.target as HTMLInputElement)}
							       placeholder={t("Slug_Placeholder")}
							       disabled={true} // TODO Article.Status is ArticleStatus.Published && Article.PublishDate < DateTimeOffset.UtcNow
							>
							</input>
						</LabelInput>
						<LabelInput label={t("PublishDate_Label")}>
							<input className="input input-bordered w-full" defaultValue={article?.publishDate?.toString()}
							       type="datetime-local" autoComplete="off"
							       disabled={true} // TODO Article.Status is ArticleStatus.Published && Article.PublishDate < DateTimeOffset.UtcNow
							>
							</input>
						</LabelInput>
					</fieldset>
					
					<fieldset className="my-6 grid grid-cols-1 lg:grid-cols-2 gap-x-8 gap-y-4" disabled={!article}>
						<div className="join join-vertical min-h-96 h-full w-full">
							<div className="flex flex-wrap gap-1 p-2 z-50 bg-base-200 sticky top-0" role="toolbar">
								<div className="join join-horizontal">
									<ToolBarButton title={t("Tools.H1_Tooltip")}
												   onClick={() => insertBeforeSelection(markdownArea.current, "# ", true)}>
										<strong>{t("Tools.H1_Label")}</strong>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.H2_Tooltip")}
												   onClick={() => insertBeforeSelection(markdownArea.current, "## ", true)}>
										<strong>{t("Tools.H2_Label")}</strong>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.H3_Tooltip")}
												   onClick={() => insertBeforeSelection(markdownArea.current, "### ", true)}>
										<strong>{t("Tools.H3_Label")}</strong>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.H4_Tooltip")}
												   onClick={() => insertBeforeSelection(markdownArea.current, "#### ", true)}>
										<strong>{t("Tools.H4_Label")}</strong>
									</ToolBarButton>
								</div>
								<div className="join join-horizontal">
									<ToolBarButton title={t("Tools.Bold_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "**")}>
										<strong>B</strong>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.Italic_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "*")}>
										<em>I</em>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.Underline_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "++")}>
										<span className="underline">U</span>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.StrikeThrough_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "~~")}>
										<del>{t("Tools.StrikeThrough_Label")}</del>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.Mark_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "==")}>
										<mark>{t("Tools.Mark_Label")}</mark>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.Mark_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "> ")}>
										| <em>{t("Tools.Cite_Label")}</em>
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
									<ToolBarButton title={t("Tools.CodeLine_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "`")}>
										<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-4 h-4">
											<path fillRule="evenodd" d="M14.447 3.026a.75.75 0 0 1 .527.921l-4.5 16.5a.75.75 0 0 1-1.448-.394l4.5-16.5a.75.75 0 0 1 .921-.527ZM16.72 6.22a.75.75 0 0 1 1.06 0l5.25 5.25a.75.75 0 0 1 0 1.06l-5.25 5.25a.75.75 0 1 1-1.06-1.06L21.44 12l-4.72-4.72a.75.75 0 0 1 0-1.06Zm-9.44 0a.75.75 0 0 1 0 1.06L2.56 12l4.72 4.72a.75.75 0 0 1-1.06 1.06L.97 12.53a.75.75 0 0 1 0-1.06l5.25-5.25a.75.75 0 0 1 1.06 0Z" clipRule="evenodd" />
										</svg>
									</ToolBarButton>
									<ToolBarButton title={t("Tools.CodeBlock_Tooltip")}
												   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "```")}>
										<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-4 h-4">
											<path fillRule="evenodd" d="M3 6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V6Zm14.25 6a.75.75 0 0 1-.22.53l-2.25 2.25a.75.75 0 1 1-1.06-1.06L15.44 12l-1.72-1.72a.75.75 0 1 1 1.06-1.06l2.25 2.25c.141.14.22.331.22.53Zm-10.28-.53a.75.75 0 0 0 0 1.06l2.25 2.25a.75.75 0 1 0 1.06-1.06L8.56 12l1.72-1.72a.75.75 0 1 0-1.06-1.06l-2.25 2.25Z" clipRule="evenodd" />
										</svg>
									</ToolBarButton>
								</div>
							</div>
							<textarea ref={markdownArea} id="tool-target" className="resize-none textarea textarea-bordered outline-none w-full flex-1 join-item" 
									  required aria-required placeholder={t("Body_Placeholder")}
									  autoComplete="off" defaultValue={article?.body}></textarea>
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
					</fieldset>

					<div className="flex gap-2 flex-wrap mt-3">
						<button type="submit" className="btn btn-primary w-full sm:btn-wide" disabled={true} // TODO implement when saving
							>
							{t("EditorSubmit")}
						</button>
						
						<a className="btn w-full sm:btn-wide" href={`/article/${article?.id}`} // TODO disable when article not exists
							>
							{t("ViewArticle_Label")}
						</a>
					</div>
				</form>
			</div>
		</>
	);
}
