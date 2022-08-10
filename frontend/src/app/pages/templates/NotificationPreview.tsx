import * as React from 'react';
import { PushPreview, PushPreviewProps, PushPreviewTarget } from 'react-push-preview';
import { DropdownItem, DropdownMenu, DropdownToggle } from 'reactstrap';
import { OverlayDropdown } from '@app/framework';
import { NotificationFormattingDto } from '@app/service';
import { texts } from '@app/texts';

type PushPreviewOption = { target: PushPreviewTarget; label: string };

const ALL_TARGETS: PushPreviewOption[] = [{
    target: 'Notifo',
    label: 'Notifo',
}, {
    target: 'DesktopChrome',
    label: 'Chrome',
}, {
    target: 'DesktopMacOS2',
    label: 'Big Sur',
}, {
    target: 'DesktopMacOS',
    label: 'MacOS',
}, {
    target: 'MobileAndroid',
    label: 'Android',
}, {
    target: 'MobileIOS',
    label: 'iOS',
}];

export interface TemplatePreviewProps {
    // The formatting.
    formatting?: NotificationFormattingDto;

    // The current language.
    language: string;
}

export const NotificationPreview = (props: TemplatePreviewProps) => {
    const { language, formatting } = props;

    const {
        body,
        confirmMode,
        confirmText,
        imageLarge,
        imageSmall,
        linkText,
        linkUrl,
        subject,
    } = formatting || {};

    const [target, setTarget] = React.useState<PushPreviewOption>(ALL_TARGETS[0]);

    const pushProps: PushPreviewProps = {
        title: subject?.[language] || texts.common.sampleSubject, website: 'Notifo',
    };

    pushProps.message = body?.[language];

    const icon = imageSmall?.[language];

    if (icon?.length > 0) {
        pushProps.iconUrl = icon;
    }

    const image = imageLarge?.[language];

    if (image?.length > 0) {
        pushProps.imageUrl = image;
    }

    if (confirmMode === 'Explicit') {
        pushProps.buttons = [{
            title: confirmText?.[language] || texts.common.confirm,
        }];
    }

    if (linkUrl?.[language]?.length > 0 && linkText?.[language]?.length > 0) {
        pushProps.linkName = linkText![language];
    }

    return (
        <>
            <PushPreview {...pushProps} target={target.target} />

            <div className='text-center mt-4'>
                <OverlayDropdown button={
                    <DropdownToggle color='primary' outline caret>
                        {target.label}
                    </DropdownToggle>
                }>
                    <DropdownMenu>
                        {ALL_TARGETS.map(x =>
                            <DropdownItem key={x.target} onClick={() => setTarget(x)}>
                                {x.label}
                            </DropdownItem>,
                        )}
                    </DropdownMenu>
                </OverlayDropdown>
            </div>
        </>
    );
};
