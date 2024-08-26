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

function httpUrlI18n(this: Yup.StringSchema) {
    // This regular expression is built on top of the one 
    // from Yup, but it also allows localhost.
    // See: https://github.com/jquense/yup/issues/224
    return this.matches(/^(?:(?:https?):\/\/|www\.)(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*\)|[-A-Z0-9+&@#\/%=~_|$?!:,.\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*\)|[A-Z0-9+&@#\/%=~_|$[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])$/i , texts.validation.httpUrlFn);
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
    Yup.addMethod(Yup.string, 'httpUrlI18n', httpUrlI18n);
    Yup.addMethod(Yup.string, 'requiredI18n', requiredI18n);
    Yup.addMethod(Yup.string, 'topicI18n', topicI18n);

    Yup.addMethod(Yup.number, 'maxI18n', maxI18N);
    Yup.addMethod(Yup.number, 'minI18n', minI18N);
    Yup.addMethod(Yup.number, 'requiredI18n', requiredI18nNumber);
}
