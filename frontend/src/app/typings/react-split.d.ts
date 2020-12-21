/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

declare module 'react-split' {
    type SplitProps = {
        children: JSX.Element[];

        direction?: 'horizontal' | 'vertical';
    };

    export default function (props: SplitProps): JSX.Element;
}
