/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Types } from '@app/framework';
import { EN } from './en';
import { TR } from './tr';

const navigatorAny = navigator as any;

let userLanguage: string =
    localStorage.getItem('language') ||
    navigator.language ||
    navigatorAny['userLanguage'];

if (userLanguage) {
    userLanguage = userLanguage.substring(0, 2);
}

let result = EN;

if (userLanguage.startsWith('tr')) {
    result = {} as any;
    Types.mergeInto(result, EN);
    Types.mergeInto(result, TR);
}

export const texts = result;
