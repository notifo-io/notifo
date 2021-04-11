/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { configureStore } from '@reduxjs/toolkit';
import { History } from 'history';
import { routerMiddleware } from 'react-router-redux';
import * as Reducers from './index';

const reducer = {
          apps: Reducers.appsReducer,
          core: Reducers.coreReducer,
emailTemplates: Reducers.emailTemplatesReducer,
        events: Reducers.eventsReducer,
  integrations: Reducers.integrationsReducer,
           log: Reducers.logReducer,
         login: Reducers.loginReducer,
         media: Reducers.mediaReducer,
       publish: Reducers.publishReducer,
 subscriptions: Reducers.subscriptionsReducer,
     templates: Reducers.templatesReducer,
         users: Reducers.usersReducer,
};

export const createAppStore = (history: History) =>
    configureStore({
        reducer,
        middleware: (getDefaultMiddleware) =>
            getDefaultMiddleware().concat(
                routerMiddleware(history),
                Reducers.loginMiddleware,
                Reducers.mediaMiddleware,
                Reducers.subscriptionsMiddleware,
                Reducers.usersMiddleware,
            ),
    });
