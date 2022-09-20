import EmojiData from '@emoji-mart/data';
import EmojiPicker from '@emoji-mart/react';
import { flip, useFloating } from '@floating-ui/react-dom';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Button, DropdownItem, DropdownMenu } from 'reactstrap';
import { ClickOutside, Icon, OverlayDropdown, useEventCallback } from '@app/framework';
import { texts } from '@app/texts';
import { MediaPicker } from './MediaPicker';

/* eslint-disable react-hooks/exhaustive-deps */

export interface PickerOptions {
    // True when emojis can be added.
    pickEmoji?: boolean;
    
    // True when media can be added.
    pickMedia?: boolean;

    // True when argument can be added.
    pickArgument?: boolean;
}

export interface PickerProps extends PickerOptions {
    // The value.
    value: string;

    // The added value.
    onPick: (value: string) => void;
}

export const Picker = (props: PickerProps) => {
    const {
        onPick,
        pickArgument,
        pickEmoji,
        pickMedia,
        value,
    } = props;

    const [openPicker, setOpenPicker] = React.useState(0);

    const { x, y, reference, floating, strategy, update } = useFloating({
        placement: 'bottom-end',
        middleware: [
            flip(),
        ],
        strategy: 'fixed',
    });

    React.useEffect(() => {
        if (openPicker !== 0) {
            const timer = setInterval(() => {
                update();
            }, 100);

            return () => {
                clearInterval(timer);
            };
        }

        return undefined;
    }, [openPicker, update]);

    React.useEffect(() => {
        update();
    }, [openPicker]);

    const doSelectUrl = useEventCallback((url: string) => {
        onPick(url);

        setOpenPicker(0);
    });

    const doSelectArgument = useEventCallback(() => {
        onPick('{{ myVariable }}');
        
        setOpenPicker(0);
    });

    const doSelectEmoji = useEventCallback((emoji: any) => {
        onPick(emoji.native);
        
        setOpenPicker(0);
    });

    const doClose = useEventCallback(() => {
        setOpenPicker(0);
    });

    const doPickMedia = useEventCallback(() => {
        setOpenPicker(1);
    });

    const doPickEmoji = useEventCallback(() => {
        setOpenPicker(2);
    });
    
    if (!pickArgument && !pickMedia) {
        return null;
    }

    return (
        <>
            <OverlayDropdown placement='right' button={
                <Button type='button' color='link' className='input-btn' innerRef={reference}>
                    <Icon className='rotate' type='add_circle' />
                </Button>
            }>
                <DropdownMenu>
                    {pickMedia &&
                        <DropdownItem onClick={doPickMedia}>
                            <Icon type='photo_size_select_actual' /> {texts.media.header}
                        </DropdownItem>
                    }

                    {pickEmoji &&
                        <DropdownItem onClick={doPickEmoji}>
                            <Icon type='insert_emoticon' /> {texts.common.emoji}
                        </DropdownItem>
                    }

                    {pickArgument &&
                        <DropdownItem onClick={doSelectArgument}>
                            <Icon type='code' /> {texts.common.property}
                        </DropdownItem>
                    }
                </DropdownMenu>
            </OverlayDropdown>
        
            {openPicker === 1 &&
                <MediaPicker
                    onClose={doClose}
                    onSelected={doSelectUrl}
                    selectedUrl={value}
                />
            }

            {openPicker === 2 &&
                <>
                    {ReactDOM.createPortal(
                        <ClickOutside isActive={true} onClickOutside={doClose}>
                            <div className='overlay emojis' ref={floating} style={{ position: strategy, top: y || 0, left: x || 0 }}>
                                <EmojiPicker data={EmojiData} onEmojiSelect={doSelectEmoji} />
                            </div>
                        </ClickOutside>,
                        document.querySelector('#portals')!,
                    )}
                </>
            }
        </>
    );
};