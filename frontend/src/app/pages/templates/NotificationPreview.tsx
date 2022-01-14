import * as React from 'react';
import { PushPreview, PushPreviewProps, PushPreviewTarget } from 'react-push-preview';
import { NotificationFormattingDto } from '@app/service';
import { texts } from '@app/texts';

export interface TemplatePreviewProps {
    // The formatting.
    formatting?: NotificationFormattingDto;

    // The target
    target?: PushPreviewTarget;

    // The current language.
    language: string;
}

export const NotificationPreview = (props: TemplatePreviewProps) => {
    const { language, formatting, target } = props;

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
        <PushPreview {...pushProps} target={target} />
    );
};
