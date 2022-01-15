/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import { Formik } from 'formik';
import * as React from 'react';
import { Form } from 'reactstrap';
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

const Template = (args: any) => {
    return (
        <Formik initialValues={{}} onSubmit={() => { }}>
            {({ handleSubmit }) => (
                <Form onSubmit={handleSubmit}>
                    <NotificationsForm.Formatting {...args} />
                </Form>
            )}
        </Formik>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    languages: [
        'de',
        'it',
        'en',
    ],
    language: 'de',
};
