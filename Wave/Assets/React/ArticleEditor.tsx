import "vite/modulepreload-polyfill";
import { useState } from 'react';

function Editor() {
    const status = "draft";

    return (
        <>
            <div className="w-full">
                <ul className="steps w-full max-w-xs">
                    <li className={`step ${status === "draft" ? "step-secondary" : ""}`}>@Localizer["Draft"]</li>
                    <li className={`step ${status === "in_review" ? "step-secondary" : ""}`}>@Localizer["InReview"]</li>
                    <li className={`step ${status === "published" ? "step-secondary" : ""}`}>@Localizer["Published"]</li>
                </ul>
            </div>
        </>
    );
}

export default Editor;