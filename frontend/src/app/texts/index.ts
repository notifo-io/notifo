/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { EN } from './en';

let userLanguage: string =
    localStorage.getItem('language') ||
    navigator.language ||
    navigator['userLanguage'];

if (userLanguage) {
    userLanguage = userLanguage.substring(0, 2);
}

export const texts = EN;
