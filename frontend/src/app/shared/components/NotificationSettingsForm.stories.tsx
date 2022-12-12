/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { Provider } from 'react-redux';
import { Col, Form, Input, Row } from 'reactstrap';
import { applyMiddleware, createStore, MiddlewareAPI } from 'redux';
import { NotificationsForm } from './NotificationSettingsForm';

export default {
    component: NotificationsForm.Formatting,
    argTypes: {
        languages: {
            table: {
                disable: true,
            },
        },
        language: {
            table: {
                disable: true,
            },
        },
        field: {
            table: {
                disable: true,
            },
        },
    },
} as ComponentMeta<any>;

const FormattingTemplate = (args: NotificationsForm.FormattingProps) => {
    const form = useForm();

    return (
        <Row>
            <Col>
                <FormProvider {...form}>
                    <Form>
                        <NotificationsForm.Formatting {...args} />
                    </Form>
                </FormProvider>
            </Col>
            <Col>
                <Input readOnly style={{ height: 600 }} type='textarea' value={JSON.stringify(form.watch(), undefined, 2)} />
            </Col>
        </Row>
    );
};

function noopMiddleware(store: MiddlewareAPI) {
    return () => () => {
        return store.getState();
    };
}

const store = createStore(x => x!,
    {
        apps: {
            apps: {
                items: [{ id: '1', configured: [] }],
            },
            appId: '1',
        },
        emailTemplates: {
            templates: {
                items: [], isLoaded: true,
            },
        },
        messagingTemplates: {
            templates: {
                items: [], isLoaded: true,
            },
        },
        smsTemplates: {
            templates: {
                items: [], isLoaded: true,
            },
        },
        integrations: {
            configured: [],
        },
    },
    applyMiddleware(noopMiddleware),
);

const SettingsTemplate = (args: NotificationsForm.SettingsProps) => {
    const form = useForm();

    return (
        <Provider store={store}>
            <Row>
                <Col>
                    <FormProvider {...form}>
                        <Form>
                            <NotificationsForm.Settings {...args} />
                        </Form>
                    </FormProvider>
                </Col>
                <Col>
                    <Input readOnly style={{ height: 600 }} type='textarea' value={JSON.stringify(form.watch(), undefined, 2)} />
                </Col>
            </Row>
        </Provider>
    );
};

export const Formatting = FormattingTemplate.bind({});

Formatting['args'] = {
    field: 'formatting',
    languages: [
        'de',
        'it',
        'en',
    ],
    language: 'de',
};

export const Settings = SettingsTemplate.bind({});

Settings['args'] = {
    field: 'formatting',
    languages: [
        'de',
        'it',
        'en',
    ],
    language: 'de',
};