/** @type {import('jest').Config} */
const config = {
    globalSetup: "./tests/_setup",
    globalTeardown: "./tests/_teardown",
    testTimeout: 5000,
};

module.exports = config;
