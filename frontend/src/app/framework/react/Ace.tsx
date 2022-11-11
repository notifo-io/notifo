/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import AceEditor from 'react-ace';

import 'ace-builds/src-noconflict/mode-html';
import 'ace-builds/src-noconflict/mode-javascript';
import 'ace-builds/src-noconflict/theme-github';

const style: React.CSSProperties = {
    fontSize: '14px',
    fontWeight: 'normal',
    width: '100%',
};

const options = {
    fontSize: 14,
};

export interface JsonDetailsProps {
    // The object.
    object: any;
}

export const JsonDetails = React.memo((props: JsonDetailsProps) => {
    const { object } = props;

    return (
        <AceEditor mode='javascript' style={style} setOptions={options} name='json-details' className='json-details'
            value={JSON.stringify(object, null, 2)}
        />
    );
});

export interface CodeDetailsProps {
    // The object.
    value: string;

    // The actual mode
    mode?: string;
}

export const CodeDetails = React.memo((props: CodeDetailsProps) => {
    const { mode, value } = props;

    return (
        <AceEditor mode={mode || 'javascript'} style={style} setOptions={options} name='code-details' className='code-details' maxLines={Infinity}
            value={value}
        />
    );
});
