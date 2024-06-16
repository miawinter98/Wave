import React, { useState, useEffect, useRef } from "react";
import { updateCharactersLeft, insertBeforeSelection, insertBeforeAndAfterSelection } from "../utilities/md_functions";
import { LabelInput, ToolBarButton } from "./Forms";
import { CategoryColor, Category, ArticleStatus, ArticleView, ArticleDto } from "../model/Models";
import { useTranslation, Trans } from 'react-i18next';
import markdownit from "markdown-it";
import markdownitmark from "markdown-it-mark";
import "groupby-polyfill/lib/polyfill.js";

const nameof = function<T>(name: keyof T) { return name; }

async function get<T>(url: string): Promise<T> {
	let response = await fetch(url, {
		method: "GET"
	});
	if (!response.ok)
		throw new Error(response.statusText);

	return response.json();
}

async function post<T>(url: string, data: T) {
	let response = await fetch(url, {
		method: "POST",
		body: JSON.stringify(data),
		headers: {
			"Content-Type": "application/json"
		}
	});
	if (!response.ok)
		throw new Error(response.statusText);
}

function Loading(message: string) {
	return <div className="grid place-items-center h-64">
		<div className="flex flex-col place-items-center">
			<p>{message}</p>
			<span className="loading loading-bars loading-lg"></span>
		</div>
	</div>
}

export default function Editor() {
	const {t} = useTranslation();
	const [notice, setNotice] = useState<string>("");
	const [dirty, setDirty] = useState(false);
	const [isPublished, setIsPublished] = useState(false);
	const [article, setArticle] = useState<ArticleView|null>(null);
	const [categories, setCategories] = useState<Category[]>([]);
	const [model, setModel] = useState<ArticleDto>({
		body: "",
		categories: [],
		id: "",
		publishDate: new Date(),
		slug: "",
		title: ""
	});

	const md = markdownit({
		html: false,
		linkify: true,
		typographer: true,
	})
		.use(markdownitmark)
		;

	function onChangeModel(event: React.ChangeEvent) {
		const {name, value} = event.target as (HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement);
		if (name == nameof<ArticleDto>("publishDate")) {
			setModel((m : ArticleDto) => ({...m, publishDate: new Date(value)}));
		} else if (event.target instanceof HTMLSelectElement) {
			const select = event.target as HTMLSelectElement;
			setModel((m : ArticleDto) => ({...m, [name]: [...select.selectedOptions].map(o => o.value)}));
		} else {
			setModel((m : ArticleDto) => ({...m, [name]: value}));
		}
		setDirty(true);
	}
	function onSubmit(event: React.FormEvent) {
		event.preventDefault();

		post("/api/article", model)
			.then(() => setDirty(false))
			.catch(err => setNotice(`Error trying to save article: ${err}`));
	}

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
				publishDate: publishDate.toString(),
				slug: "",
				status: 0,
				title: "",
				categories: []
			};
			setArticle(article);
		} else if (!article) {
			get<ArticleView>(`/api/article/${id[1]}`)
				.then(result => {
					setNotice("");
					setIsPublished(result.status >= ArticleStatus.Published && (new Date(result.publishDate) <= new Date()));
					setModel(m => ({
						...m,
						id: result.id,
						title: result.title,
						slug: result.slug,
						body: result.body,
						publishDate: new Date(result.publishDate),
						categories: result.categories.map(c => c.id),
					}));
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
				dirty &&
				<div role="alert" className="alert alert-warning sticky left-4 right-4 top-4 mb-4 z-50 rounded-sm">
					<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="size-6">
						<path fillRule="evenodd" clipRule="evenodd" d="M9.401 3.003c1.155-2 4.043-2 5.197 0l7.355 12.748c1.154 2-.29 4.5-2.599 4.5H4.645c-2.309 0-3.752-2.5-2.598-4.5L9.4 3.003ZM12 8.25a.75.75 0 0 1 .75.75v3.75a.75.75 0 0 1-1.5 0V9a.75.75 0 0 1 .75-.75Zm0 8.25a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Z" />
					</svg>
					<p>{t("editor.unsaved_changes_notice")}</p>
				</div>
			}
			{
				notice.length > 0 &&
				<div role="alert" className="alert alert-error sticky left-4 right-4 top-4 mb-4 z-50 rounded-sm">
					<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="size-6">
						<path fillRule="evenodd" clipRule="evenodd"
							  d="M9.401 3.003c1.155-2 4.043-2 5.197 0l7.355 12.748c1.154 2-.29 4.5-2.599 4.5H4.645c-2.309 0-3.752-2.5-2.598-4.5L9.4 3.003ZM12 8.25a.75.75 0 0 1 .75.75v3.75a.75.75 0 0 1-1.5 0V9a.75.75 0 0 1 .75-.75Zm0 8.25a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Z" />
					</svg>
					<p>{notice}</p>
				</div>
			}
			{
				article === null ?
					Loading(t("loading.article")) :
					<>
						<div className="w-full">
							<ul className="steps steps-vertical md:steps-horizontal">
								<li className={`step w-24 ${(article.status ?? -1) >= ArticleStatus.Draft ? "step-primary" : ""}`}>{t("Draft")}</li>
								<li className={`step w-24 ${(article.status ?? -1) >= ArticleStatus.InReview ? "step-primary" : ""}`}>{t("InReview")}</li>
								<li className={`step w-24 ${article.status === ArticleStatus.Published ? "step-primary" : ""}`}>{t("Published")}</li>
							</ul>

							<form method="post" onSubmit={onSubmit}>
								<fieldset className="grid grid-cols-1 lg:grid-cols-2 gap-x-8">
									<LabelInput label={t("Title_Label")}>
										<input className="input input-bordered w-full"
											   maxLength={256} required aria-required autoComplete="off"
											   onInput={(event: React.FormEvent<HTMLInputElement>) => updateCharactersLeft(event.target as HTMLInputElement)}
											   placeholder={t("Title_Placeholder")}
											   name={nameof<ArticleDto>("title")} value={model.title} onChange={onChangeModel} />
									</LabelInput>
									<LabelInput label={t("Category_Label")}
												className="row-span-3 order-first md:order-none">
										<select className="select select-bordered w-full" size={10} multiple={true}
												onChange={onChangeModel} name={nameof<ArticleDto>("categories")}
												defaultValue={article.categories.map(c => c.id)}>
											{
												Array.from(Map.groupBy(categories, (c: Category) => c.color) as Map<CategoryColor, Category[]>)
													.map((value, _) =>
														<optgroup className="font-bold not-italic my-3"
																  label={t(`Category.${CategoryColor[value[0]]}`)}>
															{value[1].map(c => <option key={c.id} value={c.id}>{c.name ?? "err"}</option>)}
														</optgroup>)
											}
										</select>
									</LabelInput>
									<fieldset disabled={isPublished} title={isPublished ? t("article_published_cant_edit_date_or_slug_tooltip") : undefined}>
										<LabelInput label={t("Slug_Label")}>
											<input className="input input-bordered w-full"
												   maxLength={64} autoComplete="off"
												   onInput={(event: React.FormEvent<HTMLInputElement>) => updateCharactersLeft(event.target as HTMLInputElement)}
												   placeholder={t("Slug_Placeholder")}
												   name={nameof<ArticleDto>("slug")} value={model.slug} onChange={onChangeModel} />
										</LabelInput>
										<LabelInput label={t("PublishDate_Label")}>
											<input className="input input-bordered w-full"
												   type="datetime-local" autoComplete="off"
												   name={nameof<ArticleDto>("publishDate")}
												   defaultValue={(new Date(article.publishDate).toLocaleString("sv", {timeZoneName: "short"}).substring(0, 16))}
												   onChange={onChangeModel} />
										</LabelInput>
									</fieldset>
								</fieldset>

								<fieldset className="my-6 grid grid-cols-1 lg:grid-cols-2 gap-x-8 gap-y-4">
									<div className="join join-vertical min-h-96 h-full w-full">
										<div className="flex flex-wrap gap-1 p-2 z-50 bg-base-200 sticky top-0"
											 role="toolbar">
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
															   onClick={() => insertBeforeSelection(markdownArea.current, "> ", true)}>
													| <em>{t("Tools.Cite_Label")}</em>
												</ToolBarButton>
											</div>
											<div className="join join-horizontal">
												<ToolBarButton
													onClick={() => insertBeforeSelection(markdownArea.current, "1. ", true)}>
													1.
												</ToolBarButton>
												<ToolBarButton
													onClick={() => insertBeforeSelection(markdownArea.current, "a. ", true)}>
													a.
												</ToolBarButton>
												<ToolBarButton
													onClick={() => insertBeforeSelection(markdownArea.current, "A. ", true)}>
													A.
												</ToolBarButton>
												<ToolBarButton
													onClick={() => insertBeforeSelection(markdownArea.current, "i. ", true)}>
													i.
												</ToolBarButton>
												<ToolBarButton
													onClick={() => insertBeforeSelection(markdownArea.current, "I. ", true)}>
													I.
												</ToolBarButton>
											</div>
											<div className="join join-horizontal">
												<ToolBarButton title={t("Tools.CodeLine_Tooltip")}
															   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "`")}>
													<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
														 fill="currentColor"
														 className="w-4 h-4">
														<path fillRule="evenodd"
															  d="M14.447 3.026a.75.75 0 0 1 .527.921l-4.5 16.5a.75.75 0 0 1-1.448-.394l4.5-16.5a.75.75 0 0 1 .921-.527ZM16.72 6.22a.75.75 0 0 1 1.06 0l5.25 5.25a.75.75 0 0 1 0 1.06l-5.25 5.25a.75.75 0 1 1-1.06-1.06L21.44 12l-4.72-4.72a.75.75 0 0 1 0-1.06Zm-9.44 0a.75.75 0 0 1 0 1.06L2.56 12l4.72 4.72a.75.75 0 0 1-1.06 1.06L.97 12.53a.75.75 0 0 1 0-1.06l5.25-5.25a.75.75 0 0 1 1.06 0Z"
															  clipRule="evenodd"/>
													</svg>
												</ToolBarButton>
												<ToolBarButton title={t("Tools.CodeBlock_Tooltip")}
															   onClick={() => insertBeforeAndAfterSelection(markdownArea.current, "```")}>
													<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
														 fill="currentColor"
														 className="w-4 h-4">
														<path fillRule="evenodd"
															  d="M3 6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V6Zm14.25 6a.75.75 0 0 1-.22.53l-2.25 2.25a.75.75 0 1 1-1.06-1.06L15.44 12l-1.72-1.72a.75.75 0 1 1 1.06-1.06l2.25 2.25c.141.14.22.331.22.53Zm-10.28-.53a.75.75 0 0 0 0 1.06l2.25 2.25a.75.75 0 1 0 1.06-1.06L8.56 12l1.72-1.72a.75.75 0 1 0-1.06-1.06l-2.25 2.25Z"
															  clipRule="evenodd"/>
													</svg>
												</ToolBarButton>
											</div>
										</div>
										<textarea ref={markdownArea} id="tool-target"
												  className="resize-none textarea textarea-bordered outline-none w-full flex-1 join-item"
												  required aria-required placeholder={t("Body_Placeholder")}
												  autoComplete="off"
												  name={nameof<ArticleDto>("body")} value={model.body} onChange={onChangeModel}/>
									</div>
									<div className="bg-base-200 p-2">
										<h2 className="text-2xl lg:text-4xl font-bold mb-6 hyphens-auto">
											{model.title.length < 1 ? t("Title_Placeholder") : model.title}
										</h2>
										{
											model.body.length < 1 ?
												<div className="flex flex-col gap-4">
													<div className="skeleton h-4 w-full"></div>
													<div className="skeleton h-4 w-full"></div>
													<div className="skeleton h-32 w-full"></div>
													<div className="skeleton h-4 w-full"></div>
													<div className="skeleton h-4 w-full"></div>
													<div className="skeleton h-4 w-full"></div>
												</div> :
												<div className="prose prose-neutral max-w-none hyphens-auto text-justify"
													 dangerouslySetInnerHTML={{__html: md.render(model.body)}}>

												</div>
										}
									</div>
								</fieldset>

								<div className="flex gap-2 flex-wrap mt-3">
									<button type="submit" className="btn btn-primary w-full sm:btn-wide"
											disabled={!dirty}>
										{t("EditorSubmit")}
									</button>

									<a className="btn w-full sm:btn-wide"
									   href={`/article/${article?.id}`} // TODO disable when article not exists
									>
										{t("ViewArticle_Label")}
									</a>
								</div>
							</form>
						</div>
					</>
			}
		</>
	);
}
