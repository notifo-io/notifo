// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Log;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

public record struct LogMessage(int EventCode, string Message, string System)
{
    public Exception? Exception { get; init; }

    required public string? Reason { get; init; }

    required public string? FormatText { get; init; }

    required public object[]? FormatArgs { get; init; }

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

    public static LogMessage ChannelTemplate_NotFound(string system, string? templateName)
    {
        templateName ??= "Unnamed";

        return new LogMessage(1100, $"Cannot find channel template '{templateName}'.", system)
        {
            FormatText = "Cannot find channel template '{templateName}'.",
            FormatArgs = new[] { templateName },
            Reason = $"Cannot find channel template '{templateName}'."
        };
    }

    public static LogMessage ChannelTemplate_LanguageNotFound(string system, string language, string? templateName)
    {
        templateName ??= "Unnamed";

        return new LogMessage(1101, "Cannot find language {language} in channel template '{templateName}'.", system)
        {
            FormatText = "Cannot find language {language} in channel template '{templateName}'.",
            FormatArgs = new[] { language, templateName },
            Reason = "Cannot find language {language} in channel template '{templateName}'.",
        };
    }

    public static LogMessage ChannelTemplate_ResolvedWithFallback(string system, string? templateName)
    {
        templateName ??= "Unnamed";

        return new LogMessage(1102, $"Cannot find named channel template '{templateName}', falling back to primary.", system)
        {
            FormatText = "Cannot find named template '{templateName}', falling back to primary.",
            FormatArgs = new[] { templateName },
            Reason = $"Cannot find named channel template '{templateName}', falling back to primary."
        };
    }

    public static LogMessage Events_NoTopic(string system)
    {
        return new LogMessage(1200, "Event has not topic.", system)
        {
            FormatText = "Event has no topic.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_TooOld(string system)
    {
        return new LogMessage(1200, "Event is too old and will be skipped.", system)
        {
            FormatText = "Event is too old and will be skipped.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_NoTemplateSubject(string system, string templateName)
    {
        return new LogMessage(1200, $"Template '{templateName}' has no subject.", system)
        {
            FormatText = "Template '{templateName}' has no subject.",
            FormatArgs = new[] { templateName },
            Reason = null
        };
    }

    public static LogMessage Events_AlreadyProcessed(string system)
    {
        return new LogMessage(1200, "Event with this id has already been processed.", system)
        {
            FormatText = "Event with this id has already been processed.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_NoSubscriber(string system)
    {
        return new LogMessage(1200, "Event has no subscriber.", system)
        {
            FormatText = "Event has no subscriber.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_CreationFailed(string system)
    {
        return new LogMessage(1200, "Failed to create event.", system)
        {
            FormatText = "Event with this id has already been processed.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_NoSubjectOrTemplateCode(string system)
    {
        return new LogMessage(1200, "Event with this id has already been processed.", system)
        {
            FormatText = "Event has neither a subject nor a template code.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Integration_Removed(string system)
    {
        return new LogMessage(1103, $"Integration has been removed and is not available anymore.", system)
        {
            FormatText = "Integration has been removed and is not available anymore.",
            FormatArgs = null,
            Reason = "Integration removed."
        };
    }

    public static LogMessage User_Deleted(string system, string userId)
    {
        return new LogMessage(2200, "User has been deleted", system)
        {
            FormatText = "User '{userId}' has been deleted.",
            FormatArgs = new[] { userId },
            Reason = null,
        };
    }

    public static LogMessage User_EmailRemoved(string system, string userId)
    {
        return new LogMessage(2201, "User has removed the email address.", system)
        {
            FormatText = "User '{userId}' has removed the email address.",
            FormatArgs = new[] { userId },
            Reason = "Email address removed.",
        };
    }

    public static LogMessage User_LanguageNotValid(string system, string userId, string language, string fallback)
    {
        return new LogMessage(2202, $"User has unsupported language '{language}', using '{fallback}' instead.", system)
        {
            FormatText = "User '{userId}' has unsupported language '{language}', using '{fallback}' instead.",
            FormatArgs = new[] { userId, language, fallback },
            Reason = "Email address removed.",
        };
    }

    public static LogMessage MobilePush_TokenInvalid(string system, string userId, string token)
    {
        return new LogMessage(3000, "Mobile Token became invalid.", system)
        {
            FormatText = "Mobile Token '{token}' became invalid for user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = "Mobile Token removed."
        };
    }

    public static LogMessage MobilePush_TokenRemoved(string system, string userId, string token)
    {
        return new LogMessage(3001, $"Mobile Token '{token}' has been removed.", system)
        {
            FormatText = "Mobile Token '{token}' has been removed from user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    public static LogMessage MobilePush_TokenAdded(string system, string userId, string token)
    {
        return new LogMessage(3002, $"Mobile Token '{token}' has been added.", system)
        {
            FormatText = "Mobile Token '{token}' has been added to user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    public static LogMessage WebPush_TokenInvalid(string system, string userId, string token)
    {
        return new LogMessage(4000, "Web Token became invalid.", system)
        {
            FormatText = "Web Token '{token}' became invalid for user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = "Web Token removed."
        };
    }

    public static LogMessage WebPush_TokenRemoved(string system, string userId, string token)
    {
        return new LogMessage(4001, $"Web Token '{token}' has been removed.", system)
        {
            FormatText = "Web Token '{token}' has been removed from user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    public static LogMessage WebPush_TokenAdded(string system, string userId, string token)
    {
        return new LogMessage(4002, $"Web Token '{token}' has been added.", system)
        {
            FormatText = "Web Token '{token}' has been added to user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = null
        };
    }

    public static LogMessage Messaging_CallbackError(string system, string? details)
    {
        details ??= "NoDetails";

        return new LogMessage(5000, $"Callback failed with '{details}'.", system)
        {
            FormatText = "Callback failed with '{details}'.",
            FormatArgs = new[] { details },
            Reason = "Callback failed."
        };
    }

    public static LogMessage Sms_CallbackError(string system, string? details)
    {
        details ??= "NoDetails";

        return new LogMessage(6000, $"Callback failed with '{details}'.", system)
        {
            FormatText = "Callback failed with '{details}'.",
            FormatArgs = new[] { details },
            Reason = "Callback failed."
        };
    }
}
