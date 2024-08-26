/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unused-vars */
import { PropertyFormat } from '@app/service';
import { NumberSchema, ObjectSchema, StringSchema } from 'yup';

declare module 'yup' {
    interface StringSchema {
        emailI18n(): StringSchema;

        httpUrlI18n(): StringSchema;

        requiredI18n(): StringSchema;

        topicI18n(): StringSchema;

        formatI18n(format: PropertyFormat): StringSchema;
    }

    interface NumberSchema {
        requiredI18n(): NumberSchema;

        maxI18n(value: number): NumberSchema;

        minI18n(value: number): NumberSchema;
    }

    interface ObjectSchema<TIn, TContext, TDefault, TFlags> {
        atLeastOneStringI18n(): ObjectSchema<TIn, TContext, TDefault, TFlags>;
    }
}
