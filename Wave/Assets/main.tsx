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
                    if(ui_culture && ui_culture.match(/\w\w[-\w\w]?/))
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
                    }
                },
                de: {
                    translation: {
                        Draft: "Entwurf",
                        InReview: "In Rezension",
                        Published: "Veröffentlicht"
                    }
                }
            }
        });

    const root = createRoot(domNode);
    root.render(<Editor />);
}