/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Button, Input } from 'reactstrap';
import { texts } from '@app/texts';
import { Types } from './../utils';
import { useEventCallback } from './hooks';

export interface DropZoneFiles {
    // The allowed file types.
    allowedFiles?: ReadonlyArray<string>;

    // True when only images are allowed.
    onlyImages?: boolean;
}

export interface DropZoneProps {
    // The file settings.
    files?: DropZoneFiles;

    // True to disable copy and paste.
    noPaste?: boolean;

    // Disabled.
    disabled?: boolean;

    // The selected files.
    onDrop?: (files: ReadonlyArray<File>) => void;
}

export const DropZone = (props: DropZoneProps) => {
    const { disabled, files, noPaste, onDrop } = props;

    const inputButton = React.useRef<HTMLInputElement | null>(null);
    const [, setDragCounter] = React.useState(0);

    const startDrag = useEventCallback(() => {
        setDragCounter(x => x + 1);
    });

    const stopDrag = useEventCallback((value?: number) => {
        setDragCounter(x => value || (x - 1));
    });

    const doPaste = useEventCallback((event: React.ClipboardEvent) => {
        const result = getAllowedFiles(event.clipboardData, files);

        if (result && disabled && !noPaste) {
            onDrop && onDrop(result);
        }
    });

    const doDrop = useEventCallback((event: React.DragEvent) => {
        if (hasFiles(event.dataTransfer)) {
            const result = getAllowedFiles(event.dataTransfer, files);

            if (result && !disabled) {
                onDrop && onDrop(result);
            }

            stopDrag(0);
            stopEvent(event);
        }
    });

    const doDragEnd = useEventCallback((event: React.DragEvent) => {
        const hasFile = hasAllowedFile(event.dataTransfer, files);

        if (hasFile) {
            stopDrag();
        }
    });

    const doDragEnter = useEventCallback((event: React.DragEvent) => {
        const hasFile = hasAllowedFile(event.dataTransfer, files);

        if (hasFile) {
            startDrag();
        }
    });

    const doDragOver = useEventCallback((event: React.DragEvent) => {
        const hasFile = hasFiles(event.dataTransfer);

        if (hasFile) {
            stopEvent(event);
        }
    });

    const doClick = useEventCallback(() => {
        if (inputButton.current) {
            inputButton.current.click();
        }
    });

    const doSelect = useEventCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        if (onDrop && event.target.files) {
            onDrop(getFiles(event.target.files, files));
        }
    });

    return (
        <div className='file-drop'
            onPaste={doPaste}
            onDragEnter={doDragEnter}
            onDragOver={doDragOver}
            onDragEnd={doDragEnd}
            onDragLeave={doDragEnd}
            onDrop={doDrop}>

            <Button color='success' onClick={doClick}>
                {texts.common.uploadButton}

                <Input className='file-drop-input' type='file' innerRef={inputButton} onChange={doSelect}></Input>
            </Button>

            <span className='file-drop-text'>{texts.common.uploadText}</span>
        </div>
    );
};

const ImageTypes: ReadonlyArray<string> = [
    'image/avif',
    'image/gif',
    'image/jpeg',
    'image/jpg',
    'image/png',
    'image/webp',
];

function stopEvent(event: React.DragEvent) {
    event.stopPropagation();
    event.preventDefault();
}

function getAllowedFiles(dataTransfer: DataTransfer | null, props: DropZoneFiles | undefined) {
    if (!dataTransfer || !hasFiles(dataTransfer)) {
        return null;
    }

    const files: File[] = getFiles(dataTransfer.files, props);

    if (files.length === 0) {
        for (let i = 0; i < dataTransfer.items.length; i++) {
            const file = dataTransfer.items[i].getAsFile();

            if (file && isAllowedFile(file, props)) {
                files.push(file);
            }
        }
    }

    return files.length > 0 ? files : null;
}

function getFiles(fileList: FileList, props: DropZoneFiles | undefined) {
    const files: File[] = [];

    for (let i = 0; i < fileList.length; i++) {
        const file = fileList.item(i);

        if (file && isAllowedFile(file, props)) {
            files.push(file);
        }
    }

    return files;
}

function hasAllowedFile(dataTransfer: DataTransfer | null, props: DropZoneFiles | undefined) {
    if (!dataTransfer || !hasFiles(dataTransfer)) {
        return null;
    }

    for (let i = 0; i < dataTransfer.files.length; i++) {
        const file = dataTransfer.files.item(i);

        if (file && isAllowedFile(file, props)) {
            return true;
        }
    }

    for (let i = 0; i < dataTransfer.items.length; i++) {
        const file = dataTransfer.items[i];

        if (file && isAllowedFile(file, props)) {
            return true;
        }
    }

    return false;
}

function isAllowedFile(file: { type: string }, props: DropZoneFiles | undefined) {
    return isAllowed(file, props) && isImage(file, props);
}

function isAllowed(file: { type: string }, props: DropZoneFiles | undefined) {
    return !props || !props.allowedFiles || props.allowedFiles.indexOf(file.type) >= 0;
}

function isImage(file: { type: string }, props: DropZoneFiles | undefined) {
    return !props || !props.onlyImages || ImageTypes.indexOf(file.type) >= 0;
}

function hasFiles(dataTransfer: DataTransfer): boolean {
    if (!dataTransfer || !dataTransfer.types) {
        return false;
    }

    const types: any = dataTransfer.types;

    if (Types.isFunction(types.indexOf)) {
        return types.indexOf('Files') !== -1;
    } else if (Types.isFunction(types.contains)) {
        return types.contains('Files');
    } else {
        return false;
    }
}
