/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { FormEditorOption, FormEditorProps, Forms } from '@app/framework';
import { TemplateDto } from '@app/service';

export interface TemplateInputProps extends FormEditorProps {
    // The actual templates.
    templates: ReadonlyArray<TemplateDto> | undefined;
}

export const TemplateInput = (props: TemplateInputProps) => {
    const { templates, ...other } = props;

    const options = React.useMemo(() => {
        const result: FormEditorOption<string | undefined>[] = [];

        if (templates) {
            for (const { code: label } of templates) {
                if (label) {
                    result.push({ label, value: label });
                }
            }
        }

        return result;
    }, [templates]);

    return (
        <Forms.Select {...other} options={options} />
    );
};
