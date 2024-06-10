import "vite/modulepreload-polyfill";

import { createRoot } from "react-dom/client";
import Editor from "./React/ArticleEditor";

const domNode = document.getElementById("editor");
if (domNode) {
    const root = createRoot(domNode);
    root.render(<Editor />);
}