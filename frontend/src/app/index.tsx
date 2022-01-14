/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createBrowserHistory } from 'history';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { Router } from 'react-router-dom';
import { createAppStore } from '@app/state/store';
import { App } from './App';
import './style/index.scss';

const element = document.getElementById('root') as HTMLElement;

if (element) {
    const history = createBrowserHistory();

    const store = createAppStore(history);

    const Root = (
        <Provider store={store}>
            <Router history={history}>
                <App />
            </Router>
        </Provider>
    );

    ReactDOM.render(Root, element);
}
