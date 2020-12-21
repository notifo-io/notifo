/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { apiRegister } from './api';
import { PUSH } from './push';
import { buildSDKConfig, SDKConfig } from './shared';
import { UI } from './ui';
import { isArray, JobQueue, logError, logWarn } from './utils';

import './ui/style/sdk.scss';

type Init = { config?: SDKConfig };

const queueJobs = new JobQueue();
const queueInit: Init = {};

async function init(value: any) {
    const options = buildSDKConfig(value);

    if (options) {
        await apiRegister(options);

        queueInit.config = options;
    }
}

const instance = {
    push(args: any[]) {
        if (isArray(args) && args.length >= 1) {
            switch (args[0]) {
                case 'init': {
                    queueJobs.enqueue(() => init(args[1]));
                    break;
                }
                case 'hide-notifications': {
                    queueJobs.enqueue((() => UI.destroy(args[1])));
                    break;
                }
                case 'hide-topic': {
                    queueJobs.enqueue((() => UI.destroy(args[1])));
                    break;
                }
                case 'show-notifications': {
                    queueJobs.enqueue(() => {
                        if (!queueInit.config) {
                            logError('init has not been called yet or has failed.');

                            return Promise.resolve(false);
                        }

                        return UI.setupNotifications(args[1], args[2], queueInit.config);
                    });
                    break;
                }
                case 'show-topic': {
                    queueJobs.enqueue(() => {
                        if (!queueInit.config) {
                            logError('init has not been called yet or has failed.');

                            return Promise.resolve(false);
                        }

                        return UI.setupTopic(args[1], args[2], args[3], queueInit.config);
                    });
                    break;
                }
                case 'subscribe': {
                    queueJobs.enqueue(() => {
                        if (!queueInit.config) {
                            logError('init has not been called yet or has failed.');

                            return Promise.resolve(false);
                        }

                        return PUSH.subscribe(queueInit.config, args[1]);
                    });
                    break;
                }
                case 'unsubscribe': {
                    queueJobs.enqueue(() => {
                        if (!queueInit.config) {
                            logError('init has not been called yet or has failed.');

                            return Promise.resolve(false);
                        }

                        return PUSH.unsubscribe(queueInit.config);
                    });
                    break;
                }
                default: {
                    logWarn(`Unknown command ${args[0]}`);
                }
            }
        }
    },
};

function setup() {
    const commands = window['notifo'];

    window['notifo'] = instance;

    if (isArray(commands)) {
        for (const command of commands) {
            instance.push(command);
        }
    }
}

if (document.readyState === 'complete') {
    setup();
} else {
    window.addEventListener('load', () => setup());
}
