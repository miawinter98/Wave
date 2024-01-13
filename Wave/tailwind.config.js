/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["Pages/**/*.cshtml", "Components/**/*.razor"],
    theme: {
        extend: {

        }
    },
    plugins: [require("daisyui")],
    daisyui: {
        logs: false,
        themes: [
            "light", "dark"
        ]
    }
}