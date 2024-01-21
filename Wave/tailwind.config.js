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
            heading: 'Nunito Sans',
            body: ['Noto Sans Display', ...defaultTheme.fontFamily.sans]
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
                    "primary": "#F2E530",
                    "secondary": "#ffb3c8",
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

                    "--rounded-box": "0.5rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem",
                }
            }, 
            {
                "wave-dark": {
                    "primary": "#fff133",
                    "secondary": "#DB9AAC",
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

                    "--rounded-box": "0.5rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem",
                }
            }, "wireframe"
        ],
        darkTheme: "wave-dark"
    }
}
