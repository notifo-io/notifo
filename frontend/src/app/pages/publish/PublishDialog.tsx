/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { yupResolver } from '@hookform/resolvers/yup';
import classNames from 'classnames';
import * as React from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { Button, Col, Form, Label, Modal, ModalBody, ModalFooter, ModalHeader, Nav, NavItem, NavLink, Row } from 'reactstrap';
import * as Yup from 'yup';
import { FormError, Icon, Loader, useEventCallback, usePrevious } from '@app/framework';
import { PublishDto } from '@app/service';
import { Forms, NotificationsForm, TemplateInput, TemplateVariantsInput } from '@app/shared/components';
import { CHANNELS } from '@app/shared/utils/model';
import { loadTemplates, publish, togglePublishDialog, useApp, usePublish, useTemplates } from '@app/state';
import { texts } from '@app/texts';
import { NotificationPreview } from '../templates/NotificationPreview';

const FormSchema = Yup.object({
    // Required topic name
    topic: Yup.string()
        .label(texts.common.name).requiredI18n().topicI18n(),

    // The mode (template or formatted).
    templated: Yup.boolean()
        .label(texts.common.templateMode),

    // Template code is required when templated.
    templateCode: Yup.string()
        .when('templated', (other: boolean, schema: Yup.StringSchema) =>
            (other ? schema.requiredI18n() : schema),
        )
        .label(texts.common.templateCode),

    // Array of templates variants.
    templateVariants: Yup.array(Yup.object({
        probability: Yup.number().requiredI18n().minI18n(0).maxI18n(100)
            .label(texts.common.propability),
        templateCode: Yup.string().requiredI18n()
            .label(texts.common.templateCode),
    })),

    // Subject is required when not templated.
    preformatted: Yup.object()
        .when('templated', (other: boolean, schema: Yup.ObjectSchema<any>) =>
            (other ? schema : schema.shape({ subject: Yup.object().label(texts.common.subject).atLeastOneStringI18n() })),
        )
        .label(texts.common.formatting),
});

type PublishForms = Omit<PublishDto, 'templateVariants'> & {
    templated?: boolean;
    templateVariants: { probability: number; templateCode: string }[];
};

export const PublishDialog = () => {
    const dialogOpen = usePublish(x => x.dialogOpen);

    if (!dialogOpen) {
        return null;
    }

    return (
        <PublishDialogInner />
    );
};

const PublishDialogInner = () => {
    const dispatch = useDispatch();
    const app = useApp()!;
    const appId = app.id;
    const appLanguages = app.languages;
    const dialogValues = usePublish(x => x.dialogValues || {});
    const publishing = usePublish(x => x.publishing);
    const publishingError = usePublish(x => x.publishingError);
    const templates = useTemplates(x => x.templates);
    const wasPublishing = usePrevious(publishing);
    const [language, setLanguage] = React.useState<string>(appLanguages[0]);
    const [viewFullscreen, setViewFullscreen] = React.useState<boolean>(false);
    const [viewModus, setViewModus] = React.useState(0);

    React.useEffect(() => {
        if (wasPublishing && !publishing && !publishingError) {
            toast.info(texts.publish.success);
        }
    }, [publishing, publishingError, wasPublishing]);

    React.useEffect(() => {
        if (!templates.isLoaded) {
            dispatch(loadTemplates(appId));
        }
    }, [appId, dispatch, templates.isLoaded]);

    const doCloseForm = useEventCallback(() => {
        dispatch(togglePublishDialog({ open: false }));
    });

    const doToggleFullscreen = useEventCallback(() => {
        setViewFullscreen(x => !x);
    });

    const doPublish = useEventCallback((form: PublishForms) => {
        const { templateVariants, ...other } = form;
        const params: PublishDto = other;

        if (templateVariants?.length > 0) {
            params.templateVariants = {};

            for (const variant of templateVariants) {
                params.templateVariants[variant.templateCode] = variant.probability / 100;
            }
        }

        dispatch(publish({ appId, params }));
    });

    const defaultValues: any = React.useMemo(() => {
        const result = { ...dialogValues };

        result.settings ||= {};

        for (const channel of CHANNELS) {
            result.settings[channel] ||= { send: 'Inherit', condition: 'Inherit' };
        }

        return result;
    }, [dialogValues]);

    const form = useForm<PublishForms>({ resolver: yupResolver(FormSchema), defaultValues, mode: 'onChange' });

    return (
        <Modal isOpen={true} size='lg' backdrop={false} toggle={doCloseForm} className={classNames('publish-modal', { ['fullscreen-mode']: viewFullscreen })}>
            <FormProvider {...form}>
                <Form onSubmit={form.handleSubmit(doPublish)}>
                    <ModalHeader toggle={doCloseForm}>
                        &nbsp;

                        <Nav className='nav-tabs2'>
                            <NavItem>
                                <NavLink onClick={() => setViewModus(0)} active={viewModus === 0}>{texts.common.publish}</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink onClick={() => setViewModus(1)} active={viewModus === 1}>{texts.common.channels}</NavLink>
                            </NavItem>
                        </Nav>
                        
                        <button type="button" className='fullscreen' onClick={doToggleFullscreen}>
                            <Icon type={viewFullscreen ? 'fullscreen_exit' : 'fullscreen'} />
                        </button>
                    </ModalHeader>

                    <ModalBody>
                        <Row className='flex-nowrap'>
                            <Col className='col-form'>
                                <fieldset className='mt-3' disabled={publishing}>
                                    {viewModus === 0 ? (
                                        <>
                                            <Forms.Text name='topic'
                                                label={texts.common.topic} />

                                            <Forms.Boolean name='templated'
                                                label={texts.common.templateMode} />

                                            {form.watch('templated') &&
                                                <>
                                                    <TemplateInput name='templateCode' templates={templates.items}
                                                        label={texts.common.templateCode} hints={texts.common.templateCodeHints} />

                                                    <TemplateVariantsInput name='templateVariants' templates={templates.items}
                                                        label={texts.common.variants} />
                                                </>
                                            }

                                            <NotificationsForm.Formatting
                                                onLanguageSelect={setLanguage}
                                                language={language}
                                                languages={appLanguages}
                                                field='preformatted' disabled={publishing} />

                                            <hr />

                                            <Forms.Boolean name='test'
                                                label={texts.integrations.test} />

                                            <hr />

                                            <Forms.Boolean name='silent'
                                                label={texts.common.silent} />

                                            <Forms.Textarea name='data'
                                                label={texts.common.data} hints={texts.common.dataHints} />

                                            <hr />

                                            <Forms.Number name='timeToLiveInSeconds'
                                                label={texts.common.timeToLive} min={0} max={2419200} />
                                        </>
                                    ) : (
                                        <NotificationsForm.Settings field='settings' disabled={publishing} />
                                    )}
                                </fieldset>
                            </Col>
                            
                            {viewFullscreen &&
                                <Col xs='auto' className='col-template'>
                                    <div className='publish-form-preview sticky-top'>
                                        <Label>{texts.common.preview}</Label>

                                        {form.watch('templated') ? (
                                            <div className='text-center'>{texts.common.notAvailable}</div>
                                        ) : (
                                            <NotificationPreview formatting={form.watch('preformatted')} language={language}></NotificationPreview>
                                        )}
                                    </div>
                                </Col>
                            }
                        </Row>

                        <FormError error={publishingError} />
                    </ModalBody>
                    <ModalFooter className='justify-content-between' disabled={publishing}>
                        <Button type='button' color='none' onClick={doCloseForm}>
                            {texts.common.cancel}
                        </Button>
                        <Button type='submit' color='success' disabled={publishing}>
                            <Loader light small visible={publishing} /> {texts.common.publish}
                        </Button>
                    </ModalFooter>
                </Form>
            </FormProvider>
        </Modal>
    );
};
