/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import * as Yup from 'yup';
import { Types } from '@app/framework';
import { texts } from '@app/texts';

function emailI18n(this: Yup.StringSchema) {
    return this.email(texts.validation.emailFn);
}

function urlI18n(this: Yup.StringSchema) {
    return this.url(texts.validation.urlFn);
}

function requiredI18nNumber(this: Yup.NumberSchema) {
    return this.required(texts.validation.requiredFn);
}

function maxI18N(this: Yup.NumberSchema, max: number) {
    return this.max(max, texts.validation.requiredFn);
}

function minI18N(this: Yup.NumberSchema, min: number) {
    return this.min(min, texts.validation.requiredFn);
}

function requiredI18n(this: Yup.StringSchema) {
    return this.required(texts.validation.requiredFn);
}

function topicI18n(this: Yup.StringSchema) {
    return this.matches(/^[a-z0-9\\-_]+(\/[a-z0-9\-_]+)*$/, { message: texts.validation.topicFn, excludeEmptyString: true });
}

function atLeastOneStringI18n(this: Yup.ObjectSchema<any>) {
    return this.test('at-least-one_string',
        texts.validation.atLeastOnString,
        value => {
            if (value) {
                for (const key in value) {
                    if (value.hasOwnProperty(key)) {
                        const item = value[key];

                        if (Types.isString(item) && item.trim().length > 0) {
                            return true;
                        }
                    }
                }
            }

            return false;
        });
}

export function extendYup() {
    Yup.addMethod(Yup.object, 'atLeastOneStringI18n', atLeastOneStringI18n);

    Yup.addMethod(Yup.string, 'emailI18n', emailI18n);
    Yup.addMethod(Yup.string, 'urlI18n', urlI18n);
    Yup.addMethod(Yup.string, 'requiredI18n', requiredI18n);
    Yup.addMethod(Yup.string, 'topicI18n', topicI18n);

    Yup.addMethod(Yup.number, 'maxI18n', maxI18N);
    Yup.addMethod(Yup.number, 'minI18n', minI18N);
    Yup.addMethod(Yup.number, 'requiredI18n', requiredI18nNumber);
}
