// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FluentValidation;
using NodaTime;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Events
{
    public sealed class EventMessage
    {
        public string Id { get; set; }

        public string AppId { get; set; }

        public string Topic { get; set; }

        public string? CreatorId { get; set; }

        public string? TemplateCode { get; set; }

        public string? Data { get; set; }

        public Instant Created { get; set; }

        public NotificationFormatting<LocalizedText>? Formatting { get; set; }

        public NotificationSettings? Settings { get; set; } = new NotificationSettings();

        public EventProperties Properties { get; set; }

        public Scheduling? Scheduling { get; set; }

        public bool Silent { get; set; }

        public bool Test { get; set; }

        public int? TimeToLiveInSeconds { get; set; }

        private sealed class Validator : AbstractValidator<EventMessage>
        {
            public Validator()
            {
                RuleFor(x => x.AppId).NotNull().NotEmpty();
                RuleFor(x => x.Topic).Topic();
                RuleFor(x => x.Properties).NotNull();
            }
        }

        public void Validate()
        {
            Validate<Validator>.It(this);
        }

        public override string ToString()
        {
            return TemplateCode.OrDefault(Formatting?.ToDebugString() ?? string.Empty);
        }
    }
}
