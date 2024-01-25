/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
    content: ["Pages/**/*.cshtml", "Components/**/*.razor"],
    theme: {
        extend: {
        },
        fontSize: {
            sm: '0.750rem',
            base: '1rem',
            xl: '1.333rem',
            '2xl': '1.777rem',
            '3xl': '2.369rem',
            '4xl': '3.158rem',
            '5xl': '4.210rem'
        },
        fontFamily: {
            heading: ["var(--wave-heading-font)", ...defaultTheme.fontFamily.sans],
            body: ["var(--wave-body-font)", ...defaultTheme.fontFamily.sans],
            modern: ["Tahoma", ...defaultTheme.fontFamily.sans],
            "modern-heading": ["Verdana"]
        },
        fontWeight: {
            normal: '400',
            bold: '700'
        }
    },
    plugins: [require("daisyui"), require('@tailwindcss/typography')],
    daisyui: {
        logs: false,
        themes: [
            {
                "wave-light": {
                    "--wave-heading-font": "Nunito Sans",
                    "--wave-body-font": "Noto Sans Display",

                    "primary": "#F2E530",
                    "secondary": "#ffb3c8",
                    "secondary-content": "#0D0D0D",
                    "accent": "#6FD4F2",
                    "neutral": "#A69D21",

                    "base-100": "#FAF8F2",
                    "base-200": "#CCC7B8",
                    "base-300": "#99917A",
                    "base-content": "#000000",

                    "info": "#2494F0",
                    "success": "#2CDB00",
                    "warning": "#FFF000",
                    "error": "#B3020E",
                    "error-content": "#FFFFFF",

                    "--rounded-box": "0.5rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem",
                }
            },
            {
                "wave-dark": {
                    "--wave-heading-font": "Nunito Sans",
                    "--wave-body-font": "Noto Sans Display",

                    "primary": "#fff133",
                    "secondary": "#DB9AAC",
                    "secondary-content": "#000000",
                    "accent": "#75dfff",
                    "neutral": "#A69D21",

                    "base-100": "#29141A",
                    "base-200": "#140A0D",
                    "base-300": "#0D0A0B",
                    "base-content": "#eae9fc",

                    "info": "#007EE6",
                    "success": "#1C8A00",
                    "warning": "#E3D400",
                    "error": "#610107",
                    "error-content": "#FFFFFF",

                    "--rounded-box": "0.5rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem",
                }
            },
            {
                "modern-light": {
                    "--wave-heading-font": "Tahoma",
                    "--wave-body-font": "ui-sans-serif",

                    "primary": "#1e3a8a",
                    "primary-content": "#FFFFFF",
                    "secondary": "#fafafa",
                    "secondary-content": "#1e3a8a",
                    "accent": "#93c5fd",
                    "accent-content": "#000000",
                    "neutral": "#1e40af",
                    "neutral-content": "#FFFFFF",

                    "base-100": "#FFFFFF",
                    "base-200": "#f3f4f6",
                    "base-300": "#e5e7eb",
                    "base-content": "#000000",

                    "info": "#0ea5e9",
                    "info-content": "#FFFFFF",
                    "success": "#22c55e",
                    "success-content": "#FFFFFF",
                    "warning": "#f59e0b",
                    "warning-content": "#000000",
                    "error": "#b91c1c",
                    "error-content": "#FFFFFF",

                    "--rounded-box": "0.2rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem"
                }
            },
            {
                "modern-dark": {
                    "--wave-heading-font": "Tahoma",
                    "--wave-body-font": "ui-sans-serif",
                    
                    "primary": "#60a5fa",
                    "primary-content": "#000000",
                    "secondary": "#374151",
                    "secondary-content": "#60a5fa",
                    "accent": "#93c5fd",
                    "accent-content": "#000000",
                    "neutral": "#2563eb",
                    "neutral-content": "#FFFFFF",

                    "base-100": "#4b5563",
                    "base-200": "#1f2937",
                    "base-300": "#111827",
                    "base-content": "#FFFFFF",

                    "info": "#0ea5e9",
                    "info-content": "#FFFFFF",
                    "success": "#22c55e",
                    "success-content": "#FFFFFF",
                    "warning": "#f59e0b",
                    "warning-content": "#000000",
                    "error": "#b91c1c",
                    "error-content": "#FFFFFF",

                    "--rounded-box": "0.2rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem"
                }
            }
        ],
        darkTheme: "wave-dark"
    }
}
