import path from 'path';
import { fileURLToPath } from 'url';
import preact from '@preact/preset-vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import mkcert from 'vite-plugin-mkcert';

// eslint-disable-next-line @typescript-eslint/naming-convention
const __dirname = fileURLToPath(new URL('.', import.meta.url));

// https://vitejs.dev/config/
export default defineConfig({
    resolve: {
        alias: {
            '@app': path.resolve(__dirname, './src/app'),
            '@sdk': path.resolve(__dirname, './src/sdk'),
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
    ],

    server: {
        port: 3002,

        https: true,
    },
});
