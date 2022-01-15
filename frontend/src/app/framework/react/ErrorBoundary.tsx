/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as React from 'react';
import { texts } from '@app/texts';

export interface ErrorBoundaryProps {
    // The children.
    children: React.ReactNode;

    // True if silent.
    silent?: boolean;
}

export interface ErrorBoundaryState {
    // Indicates if there is an error.
    hasError: boolean;
}

export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
    constructor(props: ErrorBoundaryProps) {
        super(props);

        this.state = { hasError: false };
    }

    public static getDerivedStateFromError() {
        return { hasError: true };
    }

    public componentWillReceiveProps() {
        this.setState({ hasError: false });
    }

    // eslint-disable-next-line class-methods-use-this
    public componentDidCatch(error: any) {
        // eslint-disable-next-line no-console
        console.log(error);
    }

    public render() {
        if (this.state.hasError && !this.props.silent) {
            return <h5>{texts.common.error}</h5>;
        }

        return this.props.children;
    }
}
