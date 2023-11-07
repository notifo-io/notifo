/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { flip, size, useFloating } from '@floating-ui/react-dom';
import classNames from 'classnames';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { ClickOutside } from './ClickOutside';
import { useEventCallback } from './hooks';

/* eslint-disable react-hooks/exhaustive-deps */

export class OverlayController {
    private static openController?: OverlayController;
    private listener?: (opened: boolean) => void;

    public isOpen = false;

    public updateOpen(isOpen: boolean) {
        this.isOpen = isOpen;

        if (isOpen) {
            if (OverlayController.openController !== this) {
                OverlayController.openController?.close();
                OverlayController.openController = this;
            }
        }
    }
    
    public open() {
        this.listener?.(true);
    }
    
    public close() {
        this.listener?.(false);
    }

    public listen(listener?: (opened: boolean) => void): () => void {
        this.listener = listener;

        return () => {
            this.listener = undefined;
        };
    }
}

export interface OverlayDropdownProps extends React.PropsWithChildren {
    // The placement relative to the button.
    placement?: 'left' | 'right';

    // The button.
    button: React.ReactNode;

    // The overlay controller.
    controller?: OverlayController;

    // True to open manually.
    openManually?: boolean;

    // True to close manually.
    closeManually?: boolean;
}

export const OverlayDropdown = (props: OverlayDropdownProps) => {
    const { 
        button, 
        children,
        closeManually,
        controller,
        openManually,
        placement,
    } = props;

    const [maxWidth, setMaxWidth] = React.useState(Number.MAX_VALUE);
    const [maxHeight, setMaxHeight] = React.useState(Number.MAX_VALUE);
    const [show, setShow] = React.useState(false);

    React.useEffect(() => {
        controller?.updateOpen(show);
    }, [controller, show]);

    const { x, y, strategy, update, refs } = useFloating({
        placement: placement === 'left' ?
            'bottom-start' :
            'bottom-end',
        middleware: [
            flip(),
            size({
                apply({ availableWidth, availableHeight }) {
                    setMaxWidth(availableWidth);
                    setMaxHeight(availableHeight);
                },
            }),
        ],
        strategy: 'fixed',
    });

    React.useEffect(() => {
        if (show) {
            const timer = setInterval(() => {
                update();
            }, 100);

            return () => {
                clearInterval(timer);
            };
        }

        return undefined;
    }, [show, update]);

    React.useEffect(() => {
        update();
    }, [show]);

    React.useEffect(() => {
        return controller?.listen(value => {
            if (value) {
                setShow(true);
            } else {
                setShow(false);
            }
        }) || (() => { });
    }, [controller]);

    const doClose = useEventCallback((event: MouseEvent) => {
        if (event.target && !(refs.reference.current as any)?.['contains'](event.target as any)) {
            setTimeout(() => {
                setShow(false);
            });
        }
    });

    const doCloseAuto = useEventCallback(() => {
        if (!closeManually) {
            setShow(false);
        }
    });

    const doOpen = useEventCallback(() => {
        if (!openManually) {
            setShow(x => !x);
        }
    });

    return (
        <>
            <span className={classNames('overlay-target', { open: show })} ref={refs.setReference} onClick={doOpen}>
                {button}
            </span>

            {show &&
                <>
                    {ReactDOM.createPortal(
                        <ClickOutside isActive={true} onClickOutside={doClose}>
                            <div className='overlay' ref={refs.setFloating} onClick={doCloseAuto} style={{ position: strategy, left: x ?? 0, top: y ?? 0, maxHeight, maxWidth }}>
                                {children}
                            </div>
                        </ClickOutside>,
                        document.querySelector('#portals')!,
                    )}
                </>
            }
        </>
    );
};
