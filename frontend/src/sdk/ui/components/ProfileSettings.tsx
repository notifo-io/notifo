/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { Fragment, h } from 'preact';

import { booleanToSend, NotificationsOptions, SDKConfig, sendToBoolean, UpdateProfile } from '@sdk/shared';
import { loadProfile, saveProfile, useDispatch, useStore } from '@sdk/ui/model';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { Icon } from './Icon';
import { Loader } from './Loader';
import { Toggle } from './Toggle';

export interface ProfileSettingsProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;

    // To toggle the profile view.
    onShowProfile?: (show: boolean) => void;
}

export const ProfileSettings = (props: ProfileSettingsProps) => {
    const {
        config,
        onShowProfile,
    } = props;

    const dispatch = useDispatch();
    const profile = useStore(x => x.profile);
    const profileTransition = useStore(x => x.profileTransition);
    const [profileToEdit, setProfileToEdit] = useState<UpdateProfile>(null);

    useEffect(() => {
        loadProfile(config, dispatch);
    }, []);

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
            saveProfile(config, profileToEdit, dispatch);
        }

        event.preventDefault();
    }, [profileToEdit]);

    const doSetEmail = useCallback((send: boolean | undefined) => {
        setChannel(profileToEdit, 'email', send);
    }, [profileToEdit]);

    const doSetPush = useCallback((send: boolean | undefined) => {
        setChannel(profileToEdit, 'webpush', send);
    }, [profileToEdit]);

    const doHideProfile = useCallback((event: Event) => {
        onShowProfile && onShowProfile(false);

        event.preventDefault();
        event.stopImmediatePropagation();
        event.stopPropagation();
    }, [onShowProfile]);

    const doChange = useCallback((event: h.JSX.TargetedEvent<HTMLInputElement> | h.JSX.TargetedEvent<HTMLSelectElement>) => {
        profileToEdit[event.currentTarget.id] = event.currentTarget.value;
    }, [profileToEdit]);

    return (
        <Fragment>
            <div>
                <button type='button' onClick={doHideProfile}>
                    <Icon type='back' size={20} />
                </button>
            </div>

            {!profileToEdit ? (
                <div class='notifo-loading'>
                    <Loader size={18} visible={true} />
                </div>
            ) : (
                <form onSubmit={doSave}>
                    {config.allowEmails &&
                        <div class='notifo-form-group'>
                            <Toggle indeterminate value={sendToBoolean(profileToEdit.settings?.email?.send)}
                                onChange={doSetEmail} />

                            <label class='notifo-form-toggle-label'>{config.texts.notifyBeEmail}</label>
                        </div>
                    }

                    <div class='notifo-form-group'>
                        <Toggle indeterminate value={sendToBoolean(profileToEdit.settings?.webpush?.send)}
                            onChange={doSetPush} />

                        <label class='notifo-form-toggle-label'>{config.texts.notifyBeWebPush}</label>
                    </div>

                    {config.allowProfile &&
                        <Fragment>
                            <hr />

                            <div class='notifo-form-group'>
                                <label class='notifo-form-label' for='fullName'>{config.texts.fullName}</label>

                                <input class='notifo-form-control' type='text' id='fullName' value={profileToEdit.fullName} onChange={doChange} />
                            </div>

                            <div class='notifo-form-group'>
                                <label class='notifo-form-label' for='emailAddress'>{config.texts.email}</label>

                                <input class='notifo-form-control' type='email' id='emailAddress' value={profileToEdit.emailAddress} onChange={doChange} />
                            </div>

                            <div class='notifo-form-group'>
                                <label class='notifo-form-label' for='preferredLanguage'>{config.texts.language}</label>

                                <select class='notifo-form-control' id='preferredLanguage' value={profile.preferredLanguage} onChange={doChange}>
                                    {profile.supportedLanguages.map(language =>
                                        <option key={language} value={language}>{language}</option>,
                                    )}
                                </select>
                            </div>

                            <div class='notifo-form-group'>
                                <label class='notifo-form-label' for='preferredTimezone'>{config.texts.timezone}</label>

                                <select class='notifo-form-control' id='preferredTimezone' value={profile.preferredTimezone} onChange={doChange}>
                                    {profile.supportedTimezones.map(timezone =>
                                        <option key={timezone} value={timezone}>{timezone}</option>,
                                    )}
                                </select>
                            </div>
                        </Fragment>
                    }

                    <hr />

                    <div class='notifo-form-group'>
                        <button class='notifo-form-button primary' type='submit'>
                            {config.texts.save}
                        </button>

                        <button class='notifo-form-button' type='submit' onClick={doHideProfile}>
                            {config.texts.cancel}
                        </button>

                        <Loader size={16} visible={profileTransition === 'InProgress'} />
                    </div>
                </form>
            )}
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
