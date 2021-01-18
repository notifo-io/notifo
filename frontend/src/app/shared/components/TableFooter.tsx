/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ListPager, ListPagerProps } from '@app/framework';
import { texts } from '@app/texts';
import * as React from 'react';
import { Col, CustomInput, Row } from 'reactstrap';

export interface TableFooterProps extends ListPagerProps  {
    // True to hide all counters.
    hideCounters?: boolean;

    // True to hide counters.
    noCounters?: boolean;

    // Triggered when to hide the counters.
    onHideCounters?: (hidden: boolean) => void;
}

export const TableFooter = (props: TableFooterProps) => {
    const { hideCounters, noCounters, onHideCounters, ...other } = props;

    const doHideCounters = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        onHideCounters && onHideCounters(event.target.checked);
    }, [onHideCounters]);

    return (
        <Row className='align-items-center'>
            {!noCounters &&
                <Col xs='auto'>
                    <CustomInput type='checkbox' id='hideCounters' checked={hideCounters} onChange={doHideCounters}
                        label={texts.common.hideDetails} />
                </Col>
            }
            <Col>
                <ListPager {...other} />
            </Col>
        </Row>
    );
};
