/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import AceEditor from 'react-ace';

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

    // True to show all counters.
    showCounters?: boolean;
}

export const JsonDetails = React.memo((props: JsonDetailsProps) => {
    const { object } = props;

    return (
        <AceEditor mode='javascript' style={style} setOptions={options} name='event-details'
            value={JSON.stringify(object, null, 2)}
        />
    );
});
