// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Notifo.Domain;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using PhoneNumbers;

namespace FluentValidation
{
    public static class ValidatorExtensions
    {
        private static readonly HashSet<string> Timezones = DateTimeZoneProviders.Tzdb.Ids.ToHashSet();

        public static IRuleBuilderOptions<T, string?> PhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                return string.IsNullOrWhiteSpace(value) || PhoneNumberUtil.IsViablePhoneNumber(value);
            }).WithMessage(Texts.ValidationPhoneNumber);
        }

        public static IRuleBuilderOptions<T, string?> Url<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                return string.IsNullOrWhiteSpace(value) || Uri.IsWellFormedUriString(value, UriKind.Absolute);
            }).WithMessage(Texts.ValidationUrl);
        }

        public static IRuleBuilderOptions<T, string?> Language<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                return value == null || Notifo.Infrastructure.Language.IsValidLanguage(value);
            }).WithMessage(Texts.ValidationLanguage);
        }

        public static IRuleBuilderOptions<T, string?> Role<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                return value == null || AppRoles.All.Contains(value);
            }).WithMessage(Texts.ValidationRole);
        }

        public static IRuleBuilderOptions<T, string?> Topic<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                return value == null || TopicId.IsValid(value);
            }).WithMessage(Texts.ValidationTopic);
        }

        public static IRuleBuilderOptions<T, string?> Timezone<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                return value == null || Timezones.Contains(value);
            }).WithMessage(Texts.ValidationTimezone);
        }
    }
}
