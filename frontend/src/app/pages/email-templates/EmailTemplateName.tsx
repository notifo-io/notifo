/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Button, Form, FormGroup, Input } from 'reactstrap';
import { useEventCallback } from '@app/framework';
import { ChannelTemplateDetailsDtoOfEmailTemplateDto } from '@app/service';
import { updateEmailTemplate } from '@app/state';
import { texts } from '@app/texts';

export interface EmailTemplateNameProps {
    // The app id.
    appId: string;

    // The template
    template?: ChannelTemplateDetailsDtoOfEmailTemplateDto;
}

export const EmailTemplateName = (props: EmailTemplateNameProps) => {
    const { appId, template } = props;

    const dispatch = useDispatch();
    const [name, setName] = React.useState<string>();

    React.useEffect(() => {
        setName(template?.name || '');
    }, [template]);

    const doSave = useEventCallback((event: React.FormEvent) => {
        if (template?.id) {
            dispatch(updateEmailTemplate({ appId, id: template.id, update: { name } }));
        }

        event.preventDefault();
    });

    const doCancel = useEventCallback(() => {
        setName(template?.name || '');
    });

    const doSetName = useEventCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setName(event.target.value);
    });

    return (
        <Form inline onSubmit={doSave}>
            <FormGroup className='mr-2'>
                <Input value={name} onChange={doSetName} disabled={!template} />
            </FormGroup>

            {template && name !== template?.name &&
                <>
                    <Button type='submit' color='primary'>
                        {texts.common.save}
                    </Button>

                    <Button color='none' onClick={doCancel}>
                        {texts.common.cancel}
                    </Button>
                </>
            }
        </Form>
    );
};
