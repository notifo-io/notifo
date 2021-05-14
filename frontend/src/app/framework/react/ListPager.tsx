/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { texts } from '@app/texts';
import * as React from 'react';
import { Col, Pagination, PaginationItem, PaginationLink, Row } from 'reactstrap';
import { ListState, Query } from './../model';

export interface ListPagerProps {
    list: ListState<any>;

    // When going to another page.
    onChange: (request: Partial<Query>) => void;
}

export const ListPager = (props: ListPagerProps) => {
    const {
        list,
        onChange,
    } = props;

    const {
        total,
        items,
        page,
        pageSize,
    } = list;

    if (items == null && total === 0) {
        return null;
    }

    const pageCount = Math.ceil(total / pageSize);

    const canGoNext = total > 0 ? page < pageCount - 1 : (items && items.length >= pageSize);
    const canGoPrev = page > 0;

    const doGoFirst = React.useCallback(() => {
        onChange({ page: 0 });
    }, []);

    const doGoPrev = React.useCallback(() => {
        onChange({ page: page - 1 });
    }, [page]);

    const doGoNext = React.useCallback(() => {
        onChange({ page: page + 1 });
    }, [page]);

    const doGoLast = React.useCallback(() => {
        onChange({ page: pageCount - 1 });
    }, [pageCount]);

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
};
