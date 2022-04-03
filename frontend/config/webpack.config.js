const webpack = require('webpack');
const helpers = require('./helpers');
const plugins = {
    // https://github.com/webpack-contrib/mini-css-extract-plugin
    MiniCssExtractPlugin: require('mini-css-extract-plugin'),
    // https://github.com/dividab/tsconfig-paths-webpack-plugin
    TsconfigPathsPlugin: require('tsconfig-paths-webpack-plugin'),
    // https://github.com/aackerman/circular-dependency-plugin
    CircularDependencyPlugin: require('circular-dependency-plugin'),
    // https://github.com/jantimon/html-webpack-plugin
    HtmlWebpackPlugin: require('html-webpack-plugin'),
    // https://webpack.js.org/plugins/terser-webpack-plugin/
    TerserPlugin: require('terser-webpack-plugin'),
    // https://github.com/NMFR/optimize-css-assets-webpack-plugin
    CssMinimizerPlugin: require('css-minimizer-webpack-plugin'),
    // https://webpack.js.org/plugins/eslint-webpack-plugin/
    ESLintPlugin: require('eslint-webpack-plugin'),
    // https://github.com/webpack-contrib/stylelint-webpack-plugin
    StylelintPlugin : require('stylelint-webpack-plugin'),
    // https://www.npmjs.com/package/webpack-bundle-analyzer
    BundleAnalyzerPlugin: require('webpack-bundle-analyzer').BundleAnalyzerPlugin,
    // https://github.com/jantimon/favicons-webpack-plugin
    FaviconsWebpackPlugin: require('favicons-webpack-plugin'),
};

module.exports = function (env) {
    const isDevServer = helpers.isDevServer();
    const isProduction = env && env.production;
    const isTests = env && env.target === 'tests';
    const isTestCoverage = env && env.coverage;
    const isAnalyzing = isProduction && env.analyze;

    // eslint-disable-next-line no-console
    console.log(`Production: ${!!isProduction}, DevServer: ${!!isDevServer}`);

    const config = {
        mode: isProduction ? 'production' : 'development',

        /**
         * Source map for Karma from the help of karma-sourcemap-loader & karma-webpack.
         *
         * See: https://webpack.js.org/configuration/devtool/
         */
        devtool: isProduction ? false : 'inline-source-map',

        output: {
            /**
             * The output directory as absolute path (required).
             *
             * See: https://webpack.js.org/configuration/output/#output-path
             */
            path: helpers.root('/build/'),

            /**
             * The filename of non-entry chunks as relative path inside the output.path directory.
             *
             * See: https://webpack.js.org/configuration/output/#output-chunkfilename
             */
            chunkFilename: '[id].[hash].chunk.js',

            /* 
             * The filename for assets.
             */
            assetModuleFilename: 'assets/[hash][ext][query]',
        },

        /**
         * Options affecting the resolving of modules.
         *
         * See: https://webpack.js.org/configuration/resolve/
         */
        resolve: {
            /**
             * An array of extensions that should be used to resolve modules.
             *
             * See: https://webpack.js.org/configuration/resolve/#resolve-extensions
             */
            extensions: ['.ts', '.tsx', '.js', '.mjs', '.css', '.scss'],
            modules: [
                helpers.root('src'),
                helpers.root('src', 'app'),
                helpers.root('src', 'app', 'style'),
                helpers.root('src', 'sdk'),
                helpers.root('node_modules'),
            ],

            plugins: [
                new plugins.TsconfigPathsPlugin({
                    configFile: 'tsconfig.json',
                }),
            ],
        },

        /**
         * Options affecting the normal modules.
         *
         * See: https://webpack.js.org/configuration/module/
         */
        module: {
            /**
             * An array of Rules which are matched to requests when modules are created.
             *
             * See: https://webpack.js.org/configuration/module/#module-rules
             */
            rules: [{
                test: /\.d\.ts?$/,
                use: [{
                    loader: 'ignore-loader',
                }],
                include: [/node_modules/],
            }, {
                test: /\.css$/,
                use: [{
                    loader: plugins.MiniCssExtractPlugin.loader,
                }, {
                    loader: 'css-loader',
                }, {
                    loader: 'postcss-loader',
                }],
            }, {
                test: /\.scss$/,
                use: [{
                    loader: plugins.MiniCssExtractPlugin.loader,
                }, {
                    loader: 'css-loader',
                }, {
                    loader: 'postcss-loader',
                }, {
                    loader: 'sass-loader',
                    options: {
                        sassOptions: {
                            quiet: true,
                        },
                    },
                }],
            }],
        },

        plugins: [
            /**
             * Puts each bundle into a file without the hash.
             * 
             * See: https://github.com/webpack-contrib/mini-css-extract-plugin
             */
            new plugins.MiniCssExtractPlugin({
                filename: '[name].css',
            }),

            new webpack.LoaderOptionsPlugin({
                options: {
                    htmlLoader: {
                        /**
                         * Define the root for images, so that we can use absolute urls.
                         * 
                         * See: https://github.com/webpack/html-loader#Advanced_Options
                         */
                        root: helpers.root('app', 'images'),
                    },
                    context: '/',
                },
            }),

            new plugins.FaviconsWebpackPlugin({
                // Favicon source logo
                logo: 'src/images/logo-square.png',
                // Favicon app title
                title: 'Notifo',
                favicons: {
                    appName: 'Notifo',
                    appDescription: 'Notifo',
                    developerName: 'Squidex UG',
                    developerUrl: 'https://notifo.io',
                    start_url: '/',
                    theme_color: '#8c84fa',
                },
            }),

            new plugins.StylelintPlugin({
                files: '**/*.scss',
            }),

            /**
             * Detect circular dependencies in app.
             * 
             * See: https://github.com/aackerman/circular-dependency-plugin
             */
            new plugins.CircularDependencyPlugin({
                exclude: /([\\\/]node_modules[\\\/])/,
                // Add errors to webpack instead of warnings
                failOnError: true,
            }),
        ],

        devServer: {
            compress: false,
            headers: {
                'Access-Control-Allow-Origin': '*',
            },
            historyApiFallback: true,
        },
    };

    if (!isTests) {
        /**
         * The entry point for the bundle. Our React app.
         *
         * See: https://webpack.js.org/configuration/entry-context/
         */
        config.entry = {
            'app': './src/app/index.tsx',
            'notifo-sdk': './src/sdk/sdk.ts',
            'notifo-sdk-worker': './src/sdk/sdk-worker.ts',
        };

        config.plugins.push(
            new plugins.HtmlWebpackPlugin({
                hash: true,
                chunks: ['app'],
                chunksSortMode: 'manual',
                template: 'src/app/index.html',
            }),
        );

        config.plugins.push(
            new plugins.HtmlWebpackPlugin({
                filename: 'demo.html',
                hash: true,
                chunks: ['app', 'notifo-sdk'],
                chunksSortMode: 'manual',
                template: 'src/sdk/demo.html',
            }),
        );


        /**
         * Specifies the name of each output file on disk.
         *
         * See: https://webpack.js.org/configuration/output/#output-filename
         */
        config.output.filename = '[name].js';

        config.plugins.push(
            new plugins.ESLintPlugin({
                files: [
                    './sdk/**/*.ts',
                    './sdk/**/*.tsx',
                    './src/**/*.ts',
                    './src/**/*.tsx',
                ],
            }),
        );
    }

    if (isProduction) {
        config.optimization = {
            minimizer: [
                new plugins.TerserPlugin({
                    terserOptions: {
                        compress: true,
                        ecma: 5,
                        mangle: true,
                        output: {
                            comments: false,
                        },
                        safari10: true,
                    },
                    extractComments: true,
                }),

                new plugins.CssMinimizerPlugin({}),
            ],
        };

        config.performance = {
            hints: false,
        };
    }

    if (isTestCoverage) {
        // Do not instrument tests.
        config.module.rules.push({
            test: /\.ts[x]?$/,
            use: [{
                loader: 'ts-loader',
            }],
            include: [/\.(e2e|spec)\.ts$/],
        });

        // Use instrument loader for all normal files.
        config.module.rules.push({
            test: /\.ts[x]?$/,
            use: [{
                loader: '@jsdevtools/coverage-istanbul-loader?esModules=true',
            }, {
                loader: 'ts-loader',
            }],
            exclude: [/\.(e2e|spec)\.ts$/],
        });
    } else {
        config.module.rules.push({
            test: /\.ts[x]?$/,
            use: [{
                loader: 'ts-loader',
            }],
        });
    }

    if (isAnalyzing) {
        config.plugins.push(new plugins.BundleAnalyzerPlugin());
    }

    return config;
};