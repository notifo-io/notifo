/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { Fragment, h } from 'preact';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { booleanToSend, NotificationsOptions, SDKConfig, sendToBoolean, UpdateProfile } from '@sdk/shared';
import { loadProfile, saveProfile, useDispatch, useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { Toggle } from './Toggle';

export interface ProfileViewProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;
}

export const ProfileView = (props: ProfileViewProps) => {
    const { config } = props;

    const dispatch = useDispatch();
    const profile = useStore(x => x.profile);
    const loaded = useStore(x => x.profileLoaded);
    const loading = useStore(x => x.profileLoading);
    const saving = useStore(x => x.profileSaving);
    const [profileToEdit, setProfileToEdit] = useState<UpdateProfile>({} as any);

    useEffect(() => {
        dispatch(loadProfile(config));
    }, [dispatch, config]);

    useEffect(() => {
        if (profile) {
            const {
                supportedLanguages,
                supportedTimezones,
                ...editable
            } = profile;

            setProfileToEdit(editable);
        }
    }, [profile]);

    const doSave = useCallback((event: Event) => {
        if (profileToEdit) {
            dispatch(saveProfile(config, profileToEdit));
        }

        event.preventDefault();
    }, [dispatch, config, profileToEdit]);

    const doSetEmail = useCallback((send: boolean | undefined) => {
        if (profileToEdit) {
            setChannel(profileToEdit, 'email', send);
        }
    }, [profileToEdit]);

    const doSetPush = useCallback((send: boolean | undefined) => {
        if (profileToEdit) {
            setChannel(profileToEdit, 'webpush', send);
        }
    }, [profileToEdit]);

    const doChange = useCallback((event: h.JSX.TargetedEvent<HTMLInputElement> | h.JSX.TargetedEvent<HTMLSelectElement>) => {
        if (profileToEdit) {
            profileToEdit[event.currentTarget.id] = event.currentTarget.value;
        }
    }, [profileToEdit]);

    const disabled = loading === 'InProgress' || saving === 'InProgress';

    return (
        <Fragment>
            <div class='notifo-list-loading'>
                <Loader size={18} visible={loading === 'InProgress' || !loaded} />
            </div>

            {loading === 'Failed' &&
                <div class='notifo-error'>{config.texts.loadingFailed}</div>
            }

            <form onSubmit={doSave}>
                {config.allowedChannels['email'] &&
                    <div class='notifo-form-group'>
                        <Toggle indeterminate value={sendToBoolean(profileToEdit.settings?.email?.send)} disabled={disabled}
                            onChange={doSetEmail} />

                        <label class='notifo-form-toggle-label'>{config.texts.notifyBeEmail}</label>
                    </div>
                }

                <div class='notifo-form-group'>
                    <Toggle indeterminate value={sendToBoolean(profileToEdit.settings?.webpush?.send)} disabled={disabled}
                        onChange={doSetPush} />

                    <label class='notifo-form-toggle-label'>{config.texts.notifyBeWebPush}</label>
                </div>

                {config.allowProfile &&
                    <Fragment>
                        <hr />

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='fullName'>{config.texts.fullName}</label>

                            <input class='notifo-form-control' type='text' id='fullName' value={profileToEdit.fullName} onChange={doChange} disabled={disabled} />
                        </div>

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='emailAddress'>{config.texts.emailAddress}</label>

                            <input class='notifo-form-control' type='email' id='emailAddress' value={profileToEdit.emailAddress} onChange={doChange} disabled={disabled} />
                        </div>

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='preferredLanguage'>{config.texts.language}</label>

                            <select class='notifo-form-control' id='preferredLanguage' value={profileToEdit.preferredLanguage} onChange={doChange} disabled={disabled}>
                                {profile?.supportedLanguages?.map(language =>
                                    <option key={language} value={language}>{language}</option>,
                                )}
                            </select>
                        </div>

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='preferredTimezone'>{config.texts.timezone}</label>

                            <select class='notifo-form-control' id='preferredTimezone' value={profileToEdit.preferredTimezone} onChange={doChange} disabled={disabled}>
                                {profile?.supportedTimezones?.map(timezone =>
                                    <option key={timezone} value={timezone}>{timezone}</option>,
                                )}
                            </select>
                        </div>
                    </Fragment>
                }

                <hr />

                <div class='notifo-form-group'>
                    {saving === 'Failed' &&
                        <div class='notifo-error'>{config.texts.savingFailed}</div>
                    }

                    <button class='notifo-form-button primary' type='submit' disabled={disabled}>
                        {config.texts.save}
                    </button>

                    <Loader size={16} visible={saving === 'InProgress'} />
                </div>
            </form>
        </Fragment>
    );
};

function setChannel(profile: UpdateProfile, channel: string, value?: boolean) {
    if (!profile.settings) {
        profile.settings = {};
    }

    const send = booleanToSend(value);

    if (!profile.settings[channel]) {
        profile.settings[channel] = { send };
    } else {
        profile.settings[channel].send = send;
    }
}
