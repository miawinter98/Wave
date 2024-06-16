import "vite/modulepreload-polyfill";
import i18n, { LanguageDetectorModule } from 'i18next';
import { initReactI18next } from 'react-i18next';

import { createRoot } from "react-dom/client";
import Editor from "./React/ArticleEditor";

const domNode = document.getElementById("editor");
if (domNode) {
    const aspnetCookieLanguageDetector : LanguageDetectorModule = {
        type: "languageDetector", detect(): string | readonly string[] | undefined {
            let cookies = document.cookie.split(";");
            let cultureCookie = cookies.find(s => s.startsWith(".AspNetCore.Culture"));

            if (cultureCookie) {
                let parts = cultureCookie.split("=", 2);
                if (parts[1]) {
                    let info = parts[1]
                        .replace("%7C", "|")
                        .replace("%3D", "=")
                        .replace("%3D", "=");
                    let cultureInfo = info.split("|", 2);
                    let ui_culture = cultureInfo[1]?.split("=", 2)[1];
                    if(ui_culture && ui_culture.match(/^\w\w(-\w\w)?$/))
                        return ui_culture;
                }

            }

            return undefined;
        }
    }

    i18n
        .use(aspnetCookieLanguageDetector)
        .use(initReactI18next)
        .init({
            fallbackLng: "en",
            interpolation: {
                escapeValue: false,
            },
            resources: {
                en: {
                    translation: {
                        Draft: "Draft",
                        InReview: "In Review",
                        Published: "Published",
                        Title_Label: "Title",
                        Title_Placeholder: "My new cheese cake recipe",
                        Slug_Label: "Article Url Part (autogenerated)",
                        Slug_Placeholder: "my-new-cheese-cake-recipe",
                        PublishDate_Label: "Publish Date",
                        PublishDate_Placeholder: "When to release this Article after approval",
                        Category_Label: "Category",
                        EditorSubmit: "Save",
                        ViewArticle_Label: "Open",

                        Body_Label: "Content",
                        Body_Placeholder: "First you preheat the oven...",
                        Tools: {
                            H1_Label: "H1",
                            H1_Tooltip: "First level heading",
                            H2_Label: "H2",
                            H2_Tooltip: "Second level heading",
                            H3_Label: "H3",
                            H3_Tooltip: "Third level heading",
                            H4_Label: "H4",
                            H4_Tooltip: "Fourth level heading",

                            Bold_Tooltip: "Make text bold",
                            Italic_Tooltip: "Make text italic",
                            StrikeThrough_Label: "del",
                            StrikeThrough_Tooltip: "Stroke through text",
                            Mark_Label: "txt",
                            Mark_Tooltip: "Mark the selected text",
                            Cite_Label: "Cite",
                            Cite_Tooltip: "Make text a citation",

                            CodeLine_Tooltip: "Mark selected text as programming code",
                            CodeBlock_Tooltip: "Insert program code block",
                        },
                        Category: {
                            Primary: "Primary Category",
                            Dangerous: "",
                            Important: "Important",
                            Informative: "Informative",
                            Secondary: "Secondary Category",
                            Default: "Regular Category",
                            Extra: "Additional Category",
                        },
                        loading: {
                            article: "Loading article...",
                        },
                        editor: {
                            unsaved_changes_notice: "You have unsaved changes, save now so you don't loose them!",
                        }
                    }
                },
                de: {
                    translation: {
                        Draft: "Entwurf",
                        InReview: "In Rezension",
                        Published: "Veröffentlicht",
                        Title_Label: "Überschrift",
                        Title_Placeholder: "Mein neues Käsekuchenrezept",
                        Slug_Label: "Artikel URL (autogeneriert)",
                        Slug_Placeholder: "mein-neues-kaesekuchenrezept",
                        PublishDate_Label: "Erscheinungsdatum",
                        PublishDate_Placeholder: "Wann dieser Artikel veröffentlicht werden soll nachdem er geprüft wurde",
                        Category_Label: "Kategorie",
                        EditorSubmit: "Speichern",
                        ViewArticle_Label: "Öffnen",

                        Body_Label: "Inhalt",
                        Body_Placeholder: "Zuerst heizt man den ofen vor...",
                        Tools: {
                            H1_Label: "Ü1",
                            H1_Tooltip: "Primärüberschrift",
                            H2_Label: "Ü2",
                            H2_Tooltip: "Sekundärüberschrift",
                            H3_Label: "Ü3",
                            H3_Tooltip: "Level 3 Überschrift",
                            H4_Label: "Ü4",
                            H4_Tooltip: "Level 4 Überschrift",

                            Bold_Tooltip: "Text hervorheben",
                            Italic_Tooltip: "Text kursiv stellen",
                            // StrikeThrough_Label -> take from en
                            StrikeThrough_Tooltip: "Text durchstreichen",
                            // Mark_Label -> take from en
                            Mark_Tooltip: "Den selektierten Text markieren",
                            Cite_Label: "Zitat",
                            Cite_Tooltip: "Text als Zitat formatieren",

                            CodeLine_Tooltip: "Selektierten text als programmcode markieren",
                            CodeBlock_Tooltip: "Programmierblock einfügen",
                        },
                        Category: {
                            Primary: "Hauptkategorie",
                            Dangerous: "",
                            Important: "Wichtig",
                            Informative: "Informativ",
                            Secondary: "Sekundäre Kategorie",
                            Default: "Reguläre Kategorie",
                            Extra: "Zusätzliche Kategorie",
                        },
                        loading: {
                            article: "Lade Artikel...",
                        },
                        editor: {
                            unsaved_changes_notice: "Sie haben ungesicherte Änderungen, speichern Sie jetzt um diese nicht zu verlieren!",
                        }
                    }
                }
            }
        });

    const root = createRoot(domNode);
    root.render(<Editor />);
}