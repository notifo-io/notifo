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
import { ViteFaviconsPlugin } from 'vite-plugin-favicon';
import defaultConfig from './vite.config.mjs';

const dirName = fileURLToPath(new URL('.', import.meta.url));

const inputs = {
    // The actual management app.
    ['app']: path.resolve(dirName, 'index.html'),
    // Build the worker separately to prevent exports.
    ['notifo-sdk']: path.resolve(dirName, 'src/sdk/sdk.ts'),
    // Build the worker separately so that it does not get any file.
    ['notifo-sdk-worker']: path.resolve(dirName, 'src/sdk/sdk-worker.ts'),
};

defaultConfig.plugins.push(
    ViteFaviconsPlugin({
        logo: './src/images/logo-square.svg',
        favicons: {
            appName: 'Notifo',
            appDescription: 'Notification Center',
            developerName: 'Squidex UG (haftungsbeschränkt)',
            developerURL: 'https://notifo.io',
            start_url: '/',
            theme_color: '#8c84fa',
        },
    }),
);

async function buildPackages() {
    await rimraf('./build');

    for (const [chunk, source] of Object.entries(inputs)) {
        // https://vitejs.dev/config/
        await build({
            publicDir: false,
            build: {
                outDir: 'build',
                rollupOptions: {
                    input: {
                        [chunk]: source,
                    },
                    output: {
                        entryFileNames: chunk => {
                            return `${chunk.name}.js`;
                        },
                        
                        assetFileNames: chunk => {
                            if (chunk.name === 'app.css' || 'notifo-sdk.css') {
                                return '[name].[ext]';
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