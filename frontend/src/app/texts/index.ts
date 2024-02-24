/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

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

export const texts = userLanguage;
