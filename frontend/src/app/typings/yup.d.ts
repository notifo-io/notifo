/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { NumberSchema, ObjectSchema, StringSchema } from 'yup';

declare module 'yup' {
    interface StringSchema {
        emailI18n(): StringSchema;

        urlI18n(): StringSchema;

        requiredI18n(): StringSchema;

        topicI18n(): StringSchema;
    }

    interface NumberSchema {
        requiredI18n(): NumberSchema;
    }

    interface ObjectSchema {
        atLeastOnStringI18n(): ObjectSchema;
    }
}
