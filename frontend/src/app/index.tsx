/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { createBrowserHistory } from 'history';
import * as ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { unstable_HistoryRouter as HistoryRouter } from 'react-router-dom';
import { createAppStore } from '@app/state/store';
import { App } from './App';
import './style/index.scss';

const element = document.getElementById('root') as HTMLElement;

if (element) {
    const initHistory = createBrowserHistory({ window });
    const initStory = createAppStore(initHistory);

    const Root = 
        <Provider store={initStory}>
            <HistoryRouter history={initHistory as any}>
                <App />
            </HistoryRouter >
        </Provider>;

    ReactDOM.render(Root, element);
}

