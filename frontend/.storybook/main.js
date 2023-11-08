const path = require('path');

const appRoot = path.resolve(__dirname, '..');

function root() {
    const newArgs = Array.prototype.slice.call(arguments, 0);

    return path.join.apply(path, [appRoot].concat(newArgs));
}

const plugins = {
    // https://github.com/dividab/tsconfig-paths-webpack-plugin
    TsconfigPathsPlugin: require('tsconfig-paths-webpack-plugin')
};

module.exports = {
    stories: [
        '../src/app/**/*.stories.@(js|jsx|ts|tsx)'
    ],

    addons: [
        '@storybook/addon-links',
        '@storybook/addon-essentials'
    ],

    framework: {
        name: '@storybook/react-webpack5',
        options: {}
    },

    webpackFinal: async (config) => {
        config.module.rules.push({
            test: /\.scss$/,
            use: [{
                loader: 'style-loader',
            }, {
                loader: 'css-loader',
            }, {
                loader: 'sass-loader',
                options: {
                    sourceMap: true,
                },
            }],
        });

        config.resolve.modules.push(root('src'));
        config.resolve.modules.push(root('src', 'app'));
        config.resolve.modules.push(root('src', 'app', 'style'));

        config.resolve.plugins = [
            new plugins.TsconfigPathsPlugin({
                configFile: root('tsconfig.json')
            }),
        ];

        return config;
    },

    docs: {
        autodocs: true
    }
}