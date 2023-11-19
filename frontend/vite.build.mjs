/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import path from 'path';
import { fileURLToPath } from 'url';
import { rimraf } from 'rimraf';
import { build } from 'vite';
import defaultConfig from './vite.config.mjs';

const dirName = fileURLToPath(new URL('.', import.meta.url));

const inputs = [{
    // The actual management app.
    ['app']: path.resolve(dirName, 'index.html'),

    // The Notifo SKD is also used by our app. Therefore we build it together.
    ['notifo-sdk']: path.resolve(dirName, 'src/sdk/sdk.ts'),
}, {
    // Build the worker separately so that it does not get any file
    ['notifo-sdk-worker']: path.resolve(dirName, 'src/sdk/sdk-worker.ts'),
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
                                return '[name].[hash].[ext]';
                            }
                        },
                    },
                },
                chunkSizeWarningLimit: 2000,
                // We empty the out directory before all builds.
                emptyOutDir: false,
            },
            configFile: false,
            ...defaultConfig,
        });
    }
}

buildPackages();