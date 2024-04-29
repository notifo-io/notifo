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

    const dispatch = useDispatch<any>();
    const [name, setName] = React.useState<{ text: string; stale: boolean }>();

    React.useEffect(() => {
        setName({ text: template?.name || '', stale: false });
    }, [template]);

    const doSave = useEventCallback((event: React.FormEvent) => {
        if (template?.id) {
            dispatch(updateEmailTemplate({ appId, id: template.id, update: { name: name?.text } }));
        }

        event.preventDefault();
    });

    const doCancel = useEventCallback(() => {
        setName({ text: template?.name || '', stale: false });
    });

    const doSetName = useEventCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setName({ text: event.target.value, stale: true });
    });

    return (
        <Form inline onSubmit={doSave}>
            <FormGroup className='mr-2'>
                <Input value={name?.text} onChange={doSetName} disabled={!template} />
            </FormGroup>

            {(template && name?.text !== template?.name && name?.stale) &&
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
