/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { yupResolver } from '@hookform/resolvers/yup';
import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { Button, Card, CardBody, Form } from 'reactstrap';
import * as Yup from 'yup';
import { FormError, Loader, useEventCallback } from '@app/framework';
import { AppDetailsDto, UpsertAppDto } from '@app/service';
import { Forms } from '@app/shared/components';
import { upsertApp, useApps, useCore } from '@app/state';
import { texts } from '@app/texts';

const FormSchema = Yup.object().shape({
    // Required name
    name: Yup.string()
        .label(texts.common.name).requiredI18n(),

    // At least one language.
    languages: Yup.array()
        .label(texts.common.languages).min(1, texts.validation.minFn),

    // Valid URL
    confirmUrl: Yup.string().nullable()
        .label(texts.app.confirmUrl).urlI18n(),
});

export interface AppSettingsProps {
    // The app details.
    appDetails: AppDetailsDto;
}

export const AppSettings = (props: AppSettingsProps) => {
    const { appDetails } = props;

    const dispatch = useDispatch();
    const languages = useCore(x => x.languages);
    const upserting = useApps(x => x.upserting);
    const upsertingError = useApps(x => x.upsertingError);

    const languageValues = React.useMemo(() => {
        return languages.map(x => x.value);
    }, [languages]);

    const doSave = useEventCallback((params: UpsertAppDto) => {
        dispatch(upsertApp({ appId: appDetails.id, params }));
    });
    
    const form = useForm<UpsertAppDto>({ resolver: yupResolver(FormSchema), defaultValues: appDetails, mode: 'onChange' });

    React.useEffect(() => {
        form.reset(appDetails);
    }, [appDetails, form]);

    return (
        <>
            <h2 className='mt-5'>{texts.common.settings}</h2>

            <FormProvider {...form}>
                <Form onSubmit={form.handleSubmit(doSave)}>
                    <Card>
                        <CardBody>
                            <fieldset disabled={upserting}>
                                <Forms.Text name='name' vertical
                                    label={texts.common.name} />

                                <Forms.Array name='languages' vertical allowedValues={languageValues}
                                    label={texts.common.languages} />
                            </fieldset>

                            <fieldset disabled={upserting}>
                                <legend>{texts.app.urls}</legend>

                                <Forms.Text name='confirmUrl' vertical placeholder={texts.common.urlPlaceholder}
                                    label={texts.app.confirmUrl} />
                            </fieldset>

                            <FormError error={upsertingError} />

                            <div className='text-right mt-2'>
                                <Button type='submit' color='success' disabled={upserting}>
                                    <Loader light small visible={upserting} /> {texts.common.save}
                                </Button>
                            </div>
                        </CardBody>
                    </Card>
                </Form>
            </FormProvider>
        </>
    );
};
