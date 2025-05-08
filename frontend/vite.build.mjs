/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import path from 'path';
import { fileURLToPath } from 'url';
import preact from '@preact/preset-vite';
import react from '@vitejs/plugin-react';
import { rimraf } from 'rimraf';
import { build } from 'vite';
import { ViteFaviconsPlugin } from 'vite-plugin-favicon2';

const dirName = fileURLToPath(new URL('.', import.meta.url));

const inputs = {
    // The actual management app.
    ['app']: {
        entry: path.resolve(dirName, 'index.html'),
        plugins: [
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
        ],
        format: undefined,
    },
    // Build the worker separately to prevent exports.
    ['notifo-sdk']: {
        entry: path.resolve(dirName, 'src/sdk/sdk.ts'),
        plugins: [
        ],
        format: 'iife',
    },
    // Build the worker separately so that it does not get any file.
    ['notifo-sdk-worker']: {
        entry: path.resolve(dirName, 'src/sdk/sdk-worker.ts'),
        plugins: [
        ],
        format: 'iife',
    },
};

async function buildPackages() {
    await rimraf('./build');

    for (const [chunk, config] of Object.entries(inputs)) {
        // https://vitejs.dev/config/
        await build({
            publicDir: false,
            build: {
                outDir: 'build',
                rollupOptions: {
                    input: {
                        [chunk]: config.entry,
                    },
                    output: {
                        format: config.format,

                        entryFileNames(chunk) {
                            return `${chunk.name}.js`;
                        },
                        
                        assetFileNames(chunk) {
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
            resolve: {
                alias: {
                    '@app': path.resolve(dirName, './src/app'),
                    '@sdk': path.resolve(dirName, './src/sdk'),
                },
            },
            plugins: [
                ...config.plugins,
                react({
                    include: 'src/**/*.tsx',
                }),
                preact({
                   
                    include: 'sdk/**/*.tsx',
                }),
                {
                    name: 'remove-pure-annotations',
                    enforce: 'pre',
                    transform(code, id) {
                        if (id.includes('node_modules/@microsoft/signalr')) {
                            return code.replace(/\/\*#__PURE__\*\//g, '');
                        }
                        
                        return null;
                    },
                },
            ],
            configFile: false,
        });
    }
}

buildPackages();