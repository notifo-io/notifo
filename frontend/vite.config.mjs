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
import { defineConfig } from 'vite';
import mkcert from 'vite-plugin-mkcert';

const dirName = fileURLToPath(new URL('.', import.meta.url));
  
// https://vitejs.dev/config/
export default defineConfig({
    resolve: {
        alias: {
            '@app': path.resolve(dirName, './src/app'),
            '@sdk': path.resolve(dirName, './src/sdk'),
        },
    },

    plugins: [
        react({
            include: 'src/**/*.tsx',
        }),
        preact({
            include: 'sdk/**/*.tsx',
        }),
        mkcert(),
        {
            name: 'full-reload-always',
            handleHotUpdate({ server }) {
                server.ws.send({ type: 'full-reload' });
                return [];
            },
        },
    ],

    test: {
        globals: true,

        browser: {
            slowHijackESM: false,

            // Browser name is required.
            name: 'chrome',
        },

        coverage: {
            provider: 'istanbul',
        },
    },

    server: {
        // The backend expects this port.
        port: 3002,

        // The certificate is automatically created by the plugin.
        https: true,
    },
});
