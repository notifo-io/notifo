/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/** @jsx h */
import { Fragment, h } from 'preact';
import { useCallback, useEffect } from 'preact/hooks';
import { NotificationsOptions, SDKConfig, sendToBoolean, setUserChannel, UpdateProfileDto } from '@sdk/shared';
import { loadProfile, saveProfile, useDispatch, useStore } from '@sdk/ui/model';
import { Loader } from './Loader';
import { Toggle } from './Toggle';
import { useMutable } from './utils';

export interface ProfileViewProps {
    // The main config.
    config: SDKConfig;

    // The options.
    options: NotificationsOptions;
}

export const ProfileView = (props: ProfileViewProps) => {
    const { config } = props;

    const dispatch = useDispatch();
    const formState = useMutable<UpdateProfileDto>({} as any);
    const formValue = formState.current;
    const loaded = useStore(x => x.profileLoaded);
    const loading = useStore(x => x.profileLoading);
    const profile = useStore(x => x.profile);
    const updating = useStore(x => x.profileUpdating);

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

            formState.set(editable);
        }
    }, [formState, profile]);

    const doSave = useCallback((event: Event) => {
        dispatch(saveProfile(config, formValue));

        event.preventDefault();
    }, [dispatch, config, formValue]);

    const doChange = useCallback((event: h.JSX.TargetedEvent<HTMLInputElement> | h.JSX.TargetedEvent<HTMLSelectElement>) => {
        formState.set(value => value[event.currentTarget.id] = event.currentTarget.value);
    }, [formState]);

    const doChangeSend = useCallback((send: boolean | undefined, channel: string) => {
        formState.set(value => setUserChannel(value, channel, send));
    }, [formState]);

    const disabled = loading === 'InProgress' || updating === 'InProgress';

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
                        <Toggle indeterminate value={sendToBoolean(formValue.settings?.email?.send)} name='email' disabled={disabled}
                            onChange={doChangeSend} />

                        <label class='notifo-form-toggle-label'>{config.texts.notifyBeEmail}</label>
                    </div>
                }

                <div class='notifo-form-group'>
                    <Toggle indeterminate value={sendToBoolean(formValue.settings?.webpush?.send)} name='webpush' disabled={disabled}
                        onChange={doChangeSend} />

                    <label class='notifo-form-toggle-label'>{config.texts.notifyBeWebPush}</label>
                </div>

                {config.allowProfile &&
                    <Fragment>
                        <hr />

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='fullName'>{config.texts.fullName}</label>

                            <input class='notifo-form-control' type='text' id='fullName' value={formValue.fullName} onChange={doChange} disabled={disabled} />
                        </div>

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='emailAddress'>{config.texts.emailAddress}</label>

                            <input class='notifo-form-control' type='email' id='emailAddress' value={formValue.emailAddress} onChange={doChange} disabled={disabled} />
                        </div>

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='preferredLanguage'>{config.texts.language}</label>

                            <select class='notifo-form-control' id='preferredLanguage' value={formValue.preferredLanguage} onChange={doChange} disabled={disabled}>
                                {profile?.supportedLanguages?.map(language =>
                                    <option key={language} value={language}>{language}</option>,
                                )}
                            </select>
                        </div>

                        <div class='notifo-form-group'>
                            <label class='notifo-form-label' for='preferredTimezone'>{config.texts.timezone}</label>

                            <select class='notifo-form-control' id='preferredTimezone' value={formValue.preferredTimezone} onChange={doChange} disabled={disabled}>
                                {profile?.supportedTimezones?.map(timezone =>
                                    <option key={timezone} value={timezone}>{timezone}</option>,
                                )}
                            </select>
                        </div>
                    </Fragment>
                }

                <hr />

                <div class='notifo-form-group'>
                    {updating === 'Failed' &&
                        <div class='notifo-error'>{config.texts.savingFailed}</div>
                    }

                    <button class='notifo-form-button primary' type='submit' disabled={disabled}>
                        {config.texts.save}
                    </button>

                    <Loader size={16} visible={updating === 'InProgress'} />
                </div>
            </form>
        </Fragment>
    );
};
