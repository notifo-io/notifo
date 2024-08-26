/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { yupResolver } from '@hookform/resolvers/yup';
import * as React from 'react';
import { FormProvider, useForm, useFormContext } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { Button, Card, CardBody, Form } from 'reactstrap';
import * as Yup from 'yup';
import { ApiValue, combineUrl, FormAlert, FormError, Loader, Toggle, useEventCallback } from '@app/framework';
import { AppDetailsDto, AuthSchemeDto, getApiUrl } from '@app/service';
import { Forms } from '@app/shared/components';
import { removeAuth, upsertAuth, useApps } from '@app/state';
import { texts } from '@app/texts';

export interface AppAuthProps {
    // The app details.
    appDetails: AppDetailsDto;
}

export const AppAuth = (props: AppAuthProps) => {
    const { appDetails } = props;

    const dispatch = useDispatch<any>();
    const auth = useApps(x => x.auth);
    const [enabled, setEnabled] = React.useState(false);

    React.useEffect(() => {
        setEnabled(!!auth?.scheme);
    }, [auth?.scheme]);

    const doUpdate = useEventCallback((value: boolean) => {
        setEnabled(value);

        if (!value) {
            dispatch(removeAuth({ appId: appDetails.id }));
        }
    });

    return (
        <>
            <h2 className='mt-5'>{texts.auth.title}</h2>

            <Card>
                <CardBody>
                    <FormAlert text={texts.auth.description} />
                    
                    <div>
                        <Toggle value={enabled} onChange={doUpdate as any}
                            label={texts.auth.enable} />
                    </div>

                    {enabled &&
                        <AuthForm appDetails={appDetails} scheme={auth?.scheme} />
                    }
                </CardBody>
            </Card>
        </>
    );
};

const FormSchema = Yup.object().shape({
    // Required domain.
    domain: Yup.string()
        .label(texts.auth.domain).requiredI18n(),

    // Required display name.
    displayName: Yup.string()
        .label(texts.auth.displayName).requiredI18n(),

    // Required client ID.
    clientId: Yup.string()
        .label(texts.auth.clientId).requiredI18n(),

    // Required client secret
    clientSecret: Yup.string()
        .label(texts.auth.clientSecret).requiredI18n(),

    // Required authory as URL.
    authority: Yup.string()
        .label(texts.auth.authority).requiredI18n(),

    // Valid URL.
    signoutRedirectUrl: Yup.string()
        .label(texts.auth.signoutRedirectUrl).httpUrlI18n(),
});

const AuthForm = ({ appDetails, scheme }: { scheme?: AuthSchemeDto } & AppAuthProps) => {
    const dispatch = useDispatch<any>();
    const updateError = useApps(x => x.updatingAuth?.error);
    const updateRunning = useApps(x => x.updatingAuth?.isRunning);

    const form = useForm<AuthSchemeDto>({ resolver: yupResolver<any>(FormSchema), mode: 'onChange' });

    const doSave = useEventCallback((params: AuthSchemeDto) => {
        dispatch(upsertAuth({ appId: appDetails.id, params }));
    });

    React.useEffect(() => {
        form.reset(scheme);
    }, [scheme, form]);

    return (
        <FormProvider {...form}>
            <Form className='mt-4' onSubmit={form.handleSubmit(doSave)}>    
                <fieldset disabled={updateRunning}>
                    <Forms.Text name='domain'
                        label={texts.auth.domain} hints={texts.auth.domainHints} />

                    <AppEmailAddress />

                    <Forms.Text name='displayName'
                        label={texts.auth.displayName} />

                    <Forms.Text name='clientId'
                        label={texts.auth.clientId} />

                    <Forms.Text name='clientSecret'
                        label={texts.auth.clientSecret} />

                    <Forms.Url name='authority'
                        label={texts.auth.authority} hints={texts.auth.authorityHints} />

                    <Forms.Url name='signoutRedirectUrl'
                        label={texts.auth.signoutRedirectUrl} />
                </fieldset>

                <fieldset className='mt-4'>
                    <Forms.Row name='none' label={texts.auth.redirectUrl} hints={texts.auth.redirectUrlHint}>
                        <ApiValue value={combineUrl(getApiUrl(), `/signin-${appDetails.id}`)} />
                    </Forms.Row>
                </fieldset>


                <FormError error={updateError} />

                <div className='d-flex justify-content-between mt-2'>
                    <AuthTestButton />

                    <Button type='submit' color='primary' disabled={updateRunning}>
                        <Loader light small visible={updateRunning} /> {texts.common.save}
                    </Button>
                </div>
            </Form>
        </FormProvider>
    );
};

const AppEmailAddress = () => {
    const { watch } = useFormContext<AuthSchemeDto>();
    const domain = watch('domain');
    
    const email = React.useMemo(() => {
        const actualDomain = domain || 'empty.com';
        
        return `name@${actualDomain}`;
    }, [domain]);

    return (
        <Forms.Row name='email'>
            <div className='truncate text-xs'>
                {texts.auth.emailLabel}: <strong className='mono text-sm'>{email}</strong>
            </div>
        </Forms.Row>
    );
};

const AuthTestButton = () => {
    const { watch } = useFormContext<AuthSchemeDto>();
    const formValues = watch();
    
    const url = React.useMemo(() => {
        const q = new URLSearchParams({
            domain: formValues.domain,
            displayName: formValues.displayName,
            clientId: formValues.clientId,
            clientSecret: formValues.clientSecret,
            authority: formValues.authority,
        });

        const url = combineUrl(getApiUrl(), `/account/logintest?${q.toString()}`);
        
        return url;
    }, [formValues]);

    return (
        <a className='btn btn-warning' target='_blank' href={url}>
            {texts.auth.testLogin}
        </a>
    );
};