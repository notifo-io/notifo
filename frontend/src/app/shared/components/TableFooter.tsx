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

export interface TableFooterProps extends ListPagerProps {
    // True to show all details.
    showDetails?: boolean;

    // True to hide the details button.
    noDetailButton?: boolean;

    // Triggered when to show the details.
    onShowDetails?: (show: boolean) => void;
}

export const TableFooter = (props: TableFooterProps) => {
    const { showDetails, noDetailButton, onShowDetails, ...other } = props;

    const doshowCounters = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        onShowDetails && onShowDetails(event.target.checked);
    }, [onShowDetails]);

    return (
        <Row className='align-items-center'>
            {!noDetailButton &&
                <Col xs='auto'>
                    <CustomInput type='checkbox' id='showCounters' checked={showDetails} onChange={doshowCounters}
                        label={texts.common.showDetails} />
                </Col>
            }
            <Col>
                <ListPager {...other} />
            </Col>
        </Row>
    );
};
