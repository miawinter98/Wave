import { defineConfig } from "vite";
import tailwindcss from "tailwindcss";
import autoprefixer from "autoprefixer";
import cssnano from "cssnano";

export default defineConfig({
	appType: 'custom',
	root: 'Assets',
	publicDir: 'public',
	build: {
		emptyOutDir: true,
		manifest: true,
		outDir: '../wwwroot',
		assetsDir: '',
		rollupOptions: {
			input: [
				'Assets/css/main.css',
				'Assets/app.ts'
			]
		},
	},
	optimizeDeps: {
		include: []
	},
	css: {
		postcss: {
			plugins: [
				tailwindcss(),
				autoprefixer(),
				cssnano({preset: "default"})
			]
		}
	}
});