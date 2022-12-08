/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
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
    const form = useForm();

    return (
        <FormProvider {...form}>
            <Form>
                <NotificationsForm.Formatting {...args} />
            </Form>
        </FormProvider>
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
