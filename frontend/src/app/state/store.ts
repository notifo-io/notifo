/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { History } from 'history';
import { routerMiddleware } from 'react-router-redux';
import { applyMiddleware, combineReducers, compose, createStore } from 'redux';
import thunk from 'redux-thunk';
import * as Reducers from './index';

const composeEnhancers = window['__REDUX_DEVTOOLS_EXTENSION_COMPOSE__'] || compose;

const appReducer = combineReducers({
                 apps: Reducers.appsReducer(),
                 core: Reducers.coreReducer(),
       emailTemplates: Reducers.emailTemplatesReducer(),
               events: Reducers.eventsReducer(),
                  log: Reducers.logReducer(),
                login: Reducers.loginReducer(),
                media: Reducers.mediaReducer(),
              publish: Reducers.publishReducer(),
        subscriptions: Reducers.subscriptionsReducer(),
            templates: Reducers.templatesReducer(),
                users: Reducers.usersReducer(),
});

export const createAppStore = (history: History) =>
    createStore(
        appReducer,
        composeEnhancers(
            applyMiddleware(
                thunk,
                routerMiddleware(history),
                Reducers.loginMiddleware(),
                Reducers.mediaMiddleware(),
                Reducers.subscriptionsMiddleware(),
                Reducers.usersMiddleware(),
            ),
        ),
    );
