/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { Col, Pagination, PaginationItem, PaginationLink, Row } from 'reactstrap';
import { texts } from '@app/texts';
import { ListState, Query } from './../model';

export interface ListPagerProps {
    list: ListState<any>;

    // Hide when nothing to navigate to.
    hideWhenUseless?: boolean;

    // When going to another page.
    onChange: (request: Partial<Query>) => void;
}

export const ListPager = React.memo((props: ListPagerProps) => {
    const {
        hideWhenUseless,
        list,
        onChange,
    } = props;

    const {
        total,
        items,
        page,
        pageSize,
    } = list;
    const pageCount = Math.ceil(total / pageSize);

    const canGoNext = total > 0 ? page < pageCount - 1 : (items && items.length >= pageSize);
    const canGoPrev = page > 0;

    const doGoFirst = () => {
        onChange({ page: 0 });
    };

    const doGoPrev = () => {
        onChange({ page: page - 1 });
    };

    const doGoNext = () => {
        onChange({ page: page + 1 });
    };

    const doGoLast = () => {
        onChange({ page: pageCount - 1 });
    };

    if (!canGoNext && !canGoPrev && hideWhenUseless) {
        return null;
    }

    if (items == null && total === 0) {
        return null;
    }

    const offset = page * pageSize;

    const itemFirst = offset + 1;
    const itemLast = offset + items!.length;

    return (
        <Row>
            <Col className='text-right'>
                <Pagination size='sm' className='pager'>
                    <PaginationItem disabled={!canGoPrev}>
                        <PaginationLink onClick={doGoFirst} first />
                    </PaginationItem>
                    <PaginationItem disabled={!canGoPrev}>
                        <PaginationLink onClick={doGoPrev} previous />
                    </PaginationItem>

                    <PaginationItem className='pager-info'>
                        {texts.common.pagination(itemFirst, itemLast, total)}
                    </PaginationItem>

                    <PaginationItem disabled={!canGoNext}>
                        <PaginationLink onClick={doGoNext} next />
                    </PaginationItem>
                    <PaginationItem disabled={!canGoNext}>
                        <PaginationLink onClick={doGoLast} last />
                    </PaginationItem>
                </Pagination>
            </Col>
        </Row>
    );
});
