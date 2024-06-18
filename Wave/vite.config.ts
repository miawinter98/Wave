import { defineConfig } from "vite";
import tailwindcss from "tailwindcss";
import autoprefixer from "autoprefixer";
import cssnano from "cssnano";
import react from "@vitejs/plugin-react-swc"

export default defineConfig({
	appType: "custom",
	base: "/dist/",
	root: "Assets",
	publicDir: "public",
	build: {
		emptyOutDir: true,
		manifest: true,
		outDir: "../wwwroot/dist",
		assetsDir: "",
		rollupOptions: {
			input: [
				"Assets/css/main.css",
				"Assets/main.tsx"
            ],
            output: {
                entryFileNames: "js/[name].[hash].js",
                chunkFileNames: "js/[name]-chunk.js",
                assetFileNames: (info) => {
                    if (info.name) {
                        if (/\.css$/.test(info.name)) {
                            return "css/[name].[hash][extname]";
                        }
                        
                        return "[name][extname]";
                    } else {
                        return "[name][extname]";
                    }
                }
            }
        },
    },
    optimizeDeps: {
        include: []
    },
    plugins: [react()],
    css: {
        postcss: {
            plugins: [
                tailwindcss(),
                autoprefixer(),
                cssnano({preset: "default"}),
            ]
        }
    }
});