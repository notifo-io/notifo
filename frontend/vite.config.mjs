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
import { ViteFaviconsPlugin } from 'vite-plugin-favicon';
import mkcert from 'vite-plugin-mkcert';

const dirName = fileURLToPath(new URL('.', import.meta.url));

const fullReloadAlways = {
    name: 'full-reload-always',
    handleHotUpdate({ server }) {
        server.ws.send({ type: 'full-reload' });
        return [];
    },
};
  
// https://vitejs.dev/config/
export default defineConfig({
    resolve: {
        alias: {
            '@app': path.resolve(dirName, './src/app'),
            '@sdk': path.resolve(dirName, './src/sdk'),
        },
    },

    plugins: [
        ...
        (process.env.NODE_ENV === 'production' ?
            [
                ViteFaviconsPlugin({
                    logo: 'src/images/logo-square.png',
                    favicons: {
                        appName: 'Notifo',
                        appDescription: 'Notifo',
                        developerName: 'Squidex UG',
                        developerUrl: 'https://notifo.io',
                        start_url: '/',
                        theme_color: '#8c84fa',
                    },
                }),
                VitePWA({
                    injectRegister: null,
                }),
            ] : []),
        react({
            include: 'src/**/*.tsx',
        }),
        preact({
            include: 'sdk/**/*.tsx',
        }),
        mkcert(),
        fullReloadAlways,
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
