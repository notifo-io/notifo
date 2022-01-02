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
        '../src/app/**/*.stories.mdx',
        '../src/app/**/*.stories.@(js|jsx|ts|tsx)'
    ],
    addons: [
        '@storybook/addon-links',
        '@storybook/addon-essentials'
    ],
    core: {
        builder: 'webpack5'
    },
    framework: '@storybook/react',
    webpackFinal: async (config) => {
        // https://github.com/storybookjs/storybook/issues/12019
        config.module.rules[0].use[0].options.plugins = [
            ['@babel/proposal-class-properties', { 'loose': true }]
        ];

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
    }
}