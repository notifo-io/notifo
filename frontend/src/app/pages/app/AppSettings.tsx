/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { FormError, Forms, Loader, usePrevious } from '@app/framework';
import { AppDetailsDto, UpsertAppDto } from '@app/service';
import { EmailVerificationStatusLabel } from '@app/shared/components';
import { upsertAppAsync, useApps, useCore } from '@app/state';
import { texts } from '@app/texts';
import { Formik } from 'formik';
import * as React from 'react';
import { useDispatch } from 'react-redux';
import { Alert, Button, Card, CardBody, Form } from 'reactstrap';
import * as Yup from 'yup';

const FormSchema = Yup.object().shape({
    // Required name
    name: Yup.string()
        .label(texts.common.name).requiredI18n(),

    // Valid email address
    emailAddress: Yup.string()
        .label(texts.common.emailAddress).emailI18n(),

    // Valid URL
    webhookURL: Yup.string()
        .label(texts.app.webhookUrl).urlI18n(),

    // At least one language.
    languages: Yup.array()
        .label(texts.common.languages).min(1, texts.validation.minFn),

    // Valid URL
    confirmUrl: Yup.string()
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
    const appDetailsPrev = usePrevious(appDetails);
    const [hint, setHint] = React.useState<string>();

    React.useEffect(() => {
        if (appDetails &&
            appDetailsPrev &&
            appDetailsPrev.emailVerificationStatus !== appDetails.emailVerificationStatus &&
            appDetails.emailVerificationStatus === 'Pending') {
            setHint(texts.app.emailVerificationHint);
        }
    }, [appDetailsPrev, appDetails]);

    const languageValues = React.useMemo(() => {
        return languages.map(x => x.value);
    }, [languages]);

    const doSave = React.useCallback((values: UpsertAppDto) => {
        dispatch(upsertAppAsync(appDetails.id, values));
    }, [appDetails.id]);

    return (
        <>
            <h2 className='mt-5'>{texts.common.settings}</h2>

            <Formik<UpsertAppDto> initialValues={appDetails} onSubmit={doSave} validationSchema={FormSchema}>
                {({ handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                        {hint &&
                            <Alert color='secondary'>
                                {hint}
                            </Alert>
                        }

                        <Card>
                            <CardBody>
                                <fieldset disabled={upserting}>
                                    <Forms.Text name='name'
                                        label={texts.common.name} />

                                    <Forms.Array name='languages' allowedValues={languageValues}
                                        label={texts.common.languages} />
                                </fieldset>

                                <fieldset disabled={upserting}>
                                    <legend>{texts.app.emailSettings}</legend>

                                    <Forms.Boolean name='allowEmail'
                                        label={texts.app.allowEmail} />

                                    <Forms.Email name='emailAddress' placeholder={texts.common.emailPlaceholder}
                                        label={texts.common.emailAddress} />

                                    <EmailVerificationStatusLabel status={appDetails.emailVerificationStatus} />

                                    <Forms.Text name='emailName'
                                        label={texts.common.emailName} />
                                </fieldset>

                                <fieldset disabled={upserting}>
                                    <legend>{texts.app.smsSettings}</legend>

                                    <Forms.Boolean name='allowSms'
                                        label={texts.app.allowSms} />
                                </fieldset>

                                <fieldset disabled={upserting}>
                                    <legend>{texts.app.firebaseSettings}</legend>

                                    <Forms.Text name='firebaseProject'
                                        label={texts.app.firebaseProject} />

                                    <Forms.TextArea name='firebaseCredential' className='firebase-credential'
                                        label={texts.app.firebaseCredential} />
                                </fieldset>

                                <fieldset disabled={upserting}>
                                    <legend>{texts.app.urls}</legend>

                                    <Forms.Text name='webhookUrl' placeholder={texts.common.urlPlaceholder}
                                        label={texts.app.webhookUrl} />

                                    <Forms.Text name='confirmUrl' placeholder={texts.common.urlPlaceholder}
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
                )}
            </Formik>
        </>
    );
};
