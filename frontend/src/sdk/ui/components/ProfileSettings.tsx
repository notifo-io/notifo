/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { h } from 'preact';
import { useCallback, useEffect, useState } from 'preact/hooks';
import { Profile } from 'src/sdk/api';
import { loadProfile, useNotifoState } from '../model';
import { NotificationsOptions, SDKConfig } from './../../shared';
import { Icon } from './Icon';
import { Loader } from './Loader';

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

    const [state, dispatch] = useNotifoState();
    const [profile, setProfile] = useState<Profile>(null);

    useEffect(() => {
        loadProfile(config, dispatch);
    }, []);

    useEffect(() => {
        if (state.profile) {
            setProfile(state.profile);
        }
    }, [state.profile]);

    const doHideProfile = useCallback(() => {
        onShowProfile && onShowProfile(false);
    }, [onShowProfile]);

    const doChange = useCallback((event: h.JSX.TargetedEvent<HTMLInputElement> | h.JSX.TargetedEvent<HTMLSelectElement>) => {
        profile[event.currentTarget.id] = event.currentTarget.value;
    }, [profile]);

    return (
        <div>
            <div>
                <button type='button' onClick={doHideProfile}>
                    <Icon type='back' size={20} />
                </button>
            </div>

            {!profile ? (
                <div class='notifo-loading'>
                    <Loader size={18} visible={true} />
                </div>
            ) : (
                <div>
                    <div class='notifo-form-group'>
                        <label for='fullName'>{config.texts.fullName}</label>

                        <input type='text' id='fullName' value={profile.fullName} onChange={doChange} />
                    </div>

                    <div class='notifo-form-group'>
                        <label for='emailAddress'>{config.texts.email}</label>

                        <input type='email' id='emailAddress' value={profile.emailAddress} onChange={doChange} />
                    </div>

                    <div class='notifo-form-group'>
                        <label for='preferredLanguage'>{config.texts.language}</label>

                        <select id='preferredLanguage' value={profile.preferredLanguage} onChange={doChange}>
                            {profile.allowedLanguages.map(language =>
                                <option key={language} value={language}>{language}</option>,
                            )}
                        </select>
                    </div>

                    <div class='notifo-form-group'>
                        <label for='preferredTimezone'>{config.texts.timezone}</label>

                        <select id='preferredTimezone' value={profile.preferredTimezone} onChange={doChange}>
                            {profile.allowedTimezones.map(timezone =>
                                <option key={timezone} value={timezone}>{timezone}</option>,
                            )}
                        </select>
                    </div>

                    <button class='notifo-button' type='submit'>
                        {state.profileTransition === 'InProgress' &&
                            <Loader size={12} visible={true} />
                        }

                        {config.texts.save}
                    </button>
                </div>
            )}
        </div>
    );
};
