/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import type { Meta, StoryObj } from '@storybook/react';
import { FormProvider, useForm } from 'react-hook-form';
import { Provider } from 'react-redux';
import { Col, Form, Input, Row } from 'reactstrap';
import { applyMiddleware, createStore, MiddlewareAPI } from 'redux';
import { NotificationsForm } from './NotificationSettingsForm';

const meta: Meta<typeof NotificationsForm.Formatting> = {
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

export default meta;
type Story = StoryObj<typeof NotificationsForm.Formatting>;

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

export const Formatting: Story = {
    args: {
        field: 'formatting',
        language: 'de',
        languages: [
            'de',
            'it',
            'en',
        ],
    },
    render: FormattingTemplate,
};

export const FormattingDisabled: Story = {
    args: {
        field: 'formatting',
        language: 'de',
        languages: [
            'de',
            'it',
            'en',
        ],
        disabled: true,
    },
    render: FormattingTemplate,
};

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

export const Settings: Story = {
    args: {
        field: 'settings',
        language: 'de',
        languages: [
            'de',
            'it',
            'en',
        ],
    },
    render: SettingsTemplate,
};

export const SettingsDisabled: Story = {
    args: {
        field: 'settings',
        language: 'de',
        languages: [
            'de',
            'it',
            'en',
        ],
        disabled: true,
    },
    render: SettingsTemplate,
};

const SchedulingTemplate = (args: NotificationsForm.SchedulingProps) => {
    const form = useForm();

    return (
        <Row>
            <Col>
                <FormProvider {...form}>
                    <Form>
                        <NotificationsForm.Scheduling {...args} />
                    </Form>
                </FormProvider>
            </Col>
            <Col>
                <Input readOnly style={{ height: 600 }} type='textarea' value={JSON.stringify(form.watch(), undefined, 2)} />
            </Col>
        </Row>
    );
};

export const Scheduling: Story = {
    args: {
        field: 'scheduling',
        language: 'de',
        languages: [
            'de',
            'it',
            'en',
        ],
    },
    render: SchedulingTemplate,
};

export const SchedulingDisabled: Story = {
    args: {
        field: 'scheduling',
        language: 'de',
        languages: [
            'de',
            'it',
            'en',
        ],
        disabled: true,
    },
    render: SchedulingTemplate,
};