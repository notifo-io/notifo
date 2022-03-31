/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable @typescript-eslint/indent */
/* eslint-disable implicit-arrow-linebreak */

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
messagingTemplates: Reducers.messagingTemplatesReducer,
     notifications: Reducers.notificationsReducer,
           publish: Reducers.publishReducer,
      smsTemplates: Reducers.smsTemplatesReducer,
     subscriptions: Reducers.subscriptionsReducer,
       systemUsers: Reducers.systemUsersReducer,
         templates: Reducers.templatesReducer,
            topics: Reducers.topicsReducer,
             users: Reducers.usersReducer,
};

export const createAppStore = (history: History) =>
    configureStore({
        reducer,
        middleware: (getDefaultMiddleware) =>
            getDefaultMiddleware().concat(
                routerMiddleware(history),
                Reducers.emailTemplatesMiddleware,
                Reducers.loginMiddleware,
                Reducers.integrationsMiddleware,
                Reducers.mediaMiddleware,
                Reducers.messagingTemplatesMiddleware,
                Reducers.smsTemplatesMiddleware,
                Reducers.subscriptionsMiddleware,
                Reducers.systemUsersMiddleware,
                Reducers.topicsMiddleware,
                Reducers.usersMiddleware,
            ),
    });
