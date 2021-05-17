/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

declare module 'react-loader-spinner' {
    type LoaderProps = {
        color?: string;
        width?: number;
        height?: number;
        type: 'Audio' | 'Ball-Triangle' | 'Bars' | 'Circles' | 'Grid' | 'Hearts' | 'Oval' | 'Puff' | 'Rings' | 'TailSpin' | 'ThreeDots';
    };

    export default function (props: LoaderProps): JSX.Element;
}
