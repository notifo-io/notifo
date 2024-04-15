/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Badge } from 'reactstrap';
import { TemplatePropertyDto } from '@app/service';

export interface TemplatePropertiesProps {
    properties: TemplatePropertyDto[];
}

export const TemplateProperties = (props: TemplatePropertiesProps) => {
    const { properties } = props;

    return (
        <div className='d-flex flex-column gap-6'>
            {properties.map(p => 
                <div key={p.path}>
                    <div style={{ width: '60px', display: 'inline-block' }}>
                        <Badge className='mr-2'>{p.type}</Badge>
                    </div>

                    <code>{p.path}</code>

                    <div className='text-muted'>
                        <small>{p.description}</small>
                    </div>
                </div>,
            )}
        </div>
    );
};
