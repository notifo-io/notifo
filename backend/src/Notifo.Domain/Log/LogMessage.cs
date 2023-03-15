// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Log;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

public record struct LogMessage(int EventCode, string Message, string System)
{
    public Exception? Exception { get; init; }

    required public string? Reason { get; init; }

    required public string? FormatText { get; init; }

    required public object[]? FormatArgs { get; init; }

    // Event Code 0000: General
    public static LogMessage General_Exception(string system, DomainException exception)
    {
        return new LogMessage(0000, exception.Message, system)
        {
            FormatText = "Internal exception.",
            FormatArgs = null,
            Reason = null,
            Exception = exception
        };
    }

    public static LogMessage General_InternalException(string system, Exception exception)
    {
        return new LogMessage(0001, exception.Message, system)
        {
            FormatText = "Internal exception.",
            FormatArgs = null,
            Reason = null,
            Exception = exception
        };
    }

    // Event Code 1000: System

    // Event Code 1100: Channel Templates
    public static LogMessage ChannelTemplate_NotFound(string system, string? templateName)
    {
        templateName ??= "<PRIMARY>";

        return new LogMessage(1100, $"Cannot find channel template '{templateName}'.", system)
        {
            FormatText = "Cannot find channel template '{templateName}'.",
            FormatArgs = new[] { templateName },
            Reason = $"Cannot find channel template '{templateName}'."
        };
    }

    public static LogMessage ChannelTemplate_LanguageNotFound(string system, string language, string? templateName)
    {
        templateName ??= "<PRIMARY>";

        return new LogMessage(1101, "Cannot find language {language} in channel template '{templateName}'.", system)
        {
            FormatText = "Cannot find language {language} in channel template '{templateName}'.",
            FormatArgs = new[] { language, templateName },
            Reason = "Cannot find language {language} in channel template '{templateName}'.",
        };
    }

    public static LogMessage ChannelTemplate_ResolvedWithFallback(string system, string? templateName)
    {
        templateName ??= "<PRIMARY>";

        return new LogMessage(1102, $"Cannot find named channel template '{templateName}', falling back to primary.", system)
        {
            FormatText = "Cannot find named template '{templateName}', falling back to primary.",
            FormatArgs = new[] { templateName },
            Reason = $"Cannot find named channel template '{templateName}', falling back to primary."
        };
    }

    public static LogMessage ChannelTemplate_TemplateError(string system, List<TemplateError> errors)
    {
        return new LogMessage(1103, "Formatting error in template.", system)
        {
            FormatText = "Cannot find named template with errors {errors}.",
            FormatArgs = new[] { string.Join(Environment.NewLine, errors) },
            Reason = "Formatting error in template."
        };
    }

    // Event Code 1200: Events
    public static LogMessage Event_NoTopic(string system)
    {
        return new LogMessage(1200, "Event has not topic.", system)
        {
            FormatText = "Event has no topic.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Event_TooOld(string system)
    {
        return new LogMessage(1201, "Event is too old and will be skipped.", system)
        {
            FormatText = "Event is too old and will be skipped.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Event_NoTemplateSubject(string system, string templateName)
    {
        return new LogMessage(1202, $"Template '{templateName}' has no subject.", system)
        {
            FormatText = "Template '{templateName}' has no subject.",
            FormatArgs = new[] { templateName },
            Reason = null
        };
    }

    public static LogMessage Event_AlreadyProcessed(string system)
    {
        return new LogMessage(1203, "Event with this id has already been processed.", system)
        {
            FormatText = "Event with this id has already been processed.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Event_NoSubscriber(string system)
    {
        return new LogMessage(1204, "Event has no subscriber.", system)
        {
            FormatText = "Event has no subscriber.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Event_CreationFailed(string system)
    {
        return new LogMessage(1205, "Failed to create event.", system)
        {
            FormatText = "Event with this id has already been processed.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Event_NoSubjectOrTemplateCode(string system)
    {
        return new LogMessage(1206, "Event with this id has already been processed.", system)
        {
            FormatText = "Event has neither a subject nor a template code.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Event_ChannelRequired(string system)
    {
        return new LogMessage(1207, "No configuration found for channel.", system)
        {
            FormatText = "No configuration found for channel.",
            FormatArgs = null,
            Reason = null
        };
    }

    // Event Code 1300: Integrations
    public static LogMessage Integration_Removed(string system)
    {
        return new LogMessage(1300, $"Integration has been removed and is not available anymore.", system)
        {
            FormatText = "Integration has been removed and is not available anymore.",
            FormatArgs = null,
            Reason = "Integration removed."
        };
    }

    // Event Code 1300: Notifications
    public static LogMessage Notification_AlreadyProcessed(string system)
    {
        return new LogMessage(1300, "Notification has already been processed.", system)
        {
            FormatText = "Notification has already been processed.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Notification_NoSubject(string system)
    {
        return new LogMessage(1301, "Notification has no subject.", system)
        {
            FormatText = "Notification has no subjectd.",
            FormatArgs = null,
            Reason = null
        };
    }

    // Event Code 2000: Users
    public static LogMessage User_Deleted(string system, string userId)
    {
        return new LogMessage(2000, "User has been deleted", system)
        {
            FormatText = "User '{userId}' has been deleted.",
            FormatArgs = new[] { userId },
            Reason = null,
        };
    }

    public static LogMessage User_EmailRemoved(string system, string userId)
    {
        return new LogMessage(2001, "User has removed the email address.", system)
        {
            FormatText = "User '{userId}' has removed the email address.",
            FormatArgs = new[] { userId },
            Reason = "Email address removed.",
        };
    }

    public static LogMessage User_LanguageNotValid(string system, string userId, string language, string fallback)
    {
        return new LogMessage(2002, $"User has unsupported language '{language}', using '{fallback}' instead.", system)
        {
            FormatText = "User '{userId}' has unsupported language '{language}', using '{fallback}' instead.",
            FormatArgs = new[] { userId, language, fallback },
            Reason = "Email address removed.",
        };
    }

    // Event Code 3000: Channels

    // Event Code 3100: MobilePush
    public static LogMessage MobilePush_TokenInvalid(string system, string userId, string token)
    {
        return new LogMessage(3100, "Mobile Token became invalid.", system)
        {
            FormatText = "Mobile Token '{token}' became invalid for user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = "Mobile Token removed."
        };
    }

    public static LogMessage MobilePush_TokenRemoved(string system, string userId, string token)
    {
        return new LogMessage(3101, $"Mobile Token '{token}' has been removed.", system)
        {
            FormatText = "Mobile Token '{token}' has been removed from user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    public static LogMessage MobilePush_TokenAdded(string system, string userId, string token)
    {
        return new LogMessage(3102, $"Mobile Token '{token}' has been added.", system)
        {
            FormatText = "Mobile Token '{token}' has been added to user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    // Event Code 3200: WebPush
    public static LogMessage WebPush_TokenInvalid(string system, string userId, string token)
    {
        return new LogMessage(3200, "Web Token became invalid.", system)
        {
            FormatText = "Web Token '{token}' became invalid for user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = "Web Token removed."
        };
    }

    public static LogMessage WebPush_TokenRemoved(string system, string userId, string token)
    {
        return new LogMessage(3201, $"Web Token '{token}' has been removed.", system)
        {
            FormatText = "Web Token '{token}' has been removed from user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    public static LogMessage WebPush_TokenAdded(string system, string userId, string token)
    {
        return new LogMessage(3203, $"Web Token '{token}' has been added.", system)
        {
            FormatText = "Web Token '{token}' has been added to user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    // Event Code 3300: Messaging
    public static LogMessage Messaging_CallbackError(string system, string? details)
    {
        details ??= "NoDetails";

        return new LogMessage(3300, $"Callback failed with '{details}'.", system)
        {
            FormatText = "Callback failed with '{details}'.",
            FormatArgs = new[] { details },
            Reason = "Callback failed."
        };
    }

    // Event Code 3400: SMS
    public static LogMessage Sms_CallbackError(string system, string? details)
    {
        details ??= "NoDetails";

        return new LogMessage(3400, $"Callback failed with '{details}'.", system)
        {
            FormatText = "Callback failed with '{details}'.",
            FormatArgs = new[] { details },
            Reason = "Callback failed."
        };
    }
}
