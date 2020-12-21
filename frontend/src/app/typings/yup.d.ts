/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { ObjectSchema, StringSchema } from 'yup';

declare module 'yup' {
    interface StringSchema {
        emailI18n(): StringSchema;

        urlI18n(): StringSchema;

        requiredI18n(): StringSchema;

        topicI18n(): StringSchema;
    }

    interface ObjectSchema {
        atLeastOnStringI18n(): ObjectSchema;
    }
}
