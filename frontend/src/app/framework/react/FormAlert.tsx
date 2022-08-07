/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import * as ReactMarkdown from 'react-markdown';
import { Alert } from 'reactstrap';
import { Icon } from './Icon';

export const FormAlert = ({ text }: { text?: string }) => {
    if (!text) {
        return null;
    }

    return (
        <Alert fade={false} className='alert-form' color='info'>
            <Icon type='info_outline' />

            <ReactMarkdown>{text}</ReactMarkdown>
        </Alert>
    );
};