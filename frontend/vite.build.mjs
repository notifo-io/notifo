import path from 'path';
import { fileURLToPath } from 'url';
import { rimraf } from 'rimraf';
import { build } from 'vite';
import defaultConfig from './vite.config.mjs';

// eslint-disable-next-line @typescript-eslint/naming-convention
const __dirname = fileURLToPath(new URL('.', import.meta.url));

const inputs = [{
    ['app']: path.resolve(__dirname, 'index.html'),
    // The notifo SKD is also used by our app. Therefore we build it together.
    ['notifo-sdk']: path.resolve(__dirname, 'src/sdk/sdk.ts'),
}, {
    // Build the worker separately so that it does not get any file
    ['notifo-sdk-worker']: path.resolve(__dirname, 'src/sdk/sdk-worker.ts'),
}];

async function buildPackages() {
    await rimraf('./build');

    for (const input of inputs) {
        // https://vitejs.dev/config/
        await build({
            publicDir: false,
            build: {
                outDir: 'build',
                rollupOptions: {
                    input,
                    output: {
                        entryFileNames: chunk => {
                            if (chunk.name === 'index') {
                                return 'app.js';
                            } else {
                                return `${chunk.name}.js`;
                            }
                        },
                        
                        assetFileNames: chunk => {
                            if (chunk.name === 'index.css') {
                                return 'app.css';
                            } else if (chunk.name === 'sdk.css') {
                                return 'notifo-sdk.css';
                            } else {
                                return `${chunk.name}.css`;
                            }
                        },
                    },
                },
                // We empty once.
                emptyOutDir: false,
                chunkSizeWarningLimit: 2000,
            },
            configFile: false,
            ...defaultConfig,
        });
    }
}

buildPackages();