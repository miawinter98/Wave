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
                    "primary": "#fff133",
                    "secondary": "#ffb3c8",
                    "accent": "#75dfff",
                    "neutral": "#75dfff",

                    "base-100": "#FAF8F2",
                    "base-200": "#CCC7B8",
                    "base-300": "#99917A",
                    "base-content": "#000000",

                    "--rounded-box": "0.5rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem",
                }
            }, 
            {
                "wave-dark": {
                    "primary": "#fff133",
                    "secondary": "#ffb3c8",
                    "accent": "#75dfff",
                    "neutral": "#75dfff",

                    "base-100": "#29141A",
                    "base-200": "#140A0D",
                    "base-300": "#0D0A0B",
                    "base-content": "#eae9fc",

                    "--rounded-box": "0.5rem",
                    "--rounded-btn": "0.2rem",
                    "--rounded-badge": "0.2rem",
                }
            }, "wireframe"
        ],
        darkTheme: "wave-dark"
    }
}
