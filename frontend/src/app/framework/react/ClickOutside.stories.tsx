/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ComponentMeta } from '@storybook/react';
import * as React from 'react';
import { Card, CardBody } from 'reactstrap';
import { ClickOutside } from './ClickOutside';

export default {
    component: ClickOutside,
} as ComponentMeta<typeof ClickOutside>;

const Template = (args: any) => {
    return (
        <ClickOutside {...args}>
            <Card>
                <CardBody>Inner</CardBody>
            </Card>
        </ClickOutside>
    );
};

export const Default = Template.bind({});

Default['args'] = {
    isActive: true,
};