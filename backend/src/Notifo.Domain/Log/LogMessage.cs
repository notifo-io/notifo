// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Log;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

public record struct LogMessage(int EventCode, string System, string Text)
{
    public Exception? Exception { get; init; }

    required public string? Reason { get; init; }

    required public string? FormatText { get; init; }

    required public object[]? FormatArgs { get; init; }

    public static LogMessage General_Exception(string system, DomainException exception)
    {
        return new LogMessage(0000, system, exception.Message)
        {
            FormatText = "Internal exception.",
            FormatArgs = null,
            Reason = null,
            Exception = exception
        };
    }

    public static LogMessage General_InternalException(string system, Exception exception)
    {
        return new LogMessage(0001, system, exception.Message)
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

        return new LogMessage(1100, system, $"Cannot find channel template '{templateName}'.")
        {
            FormatText = "Cannot find channel template '{templateName}'.",
            FormatArgs = new[] { templateName },
            Reason = $"Cannot find channel template '{templateName}'."
        };
    }

    public static LogMessage ChannelTemplate_LanguageNotFound(string system, string language, string? templateName)
    {
        templateName ??= "Unnamed";

        return new LogMessage(1101, system, "Cannot find language {language} in channel template '{templateName}'.")
        {
            FormatText = "Cannot find language {language} in channel template '{templateName}'.",
            FormatArgs = new[] { language, templateName },
            Reason = "Cannot find language {language} in channel template '{templateName}'.",
        };
    }

    public static LogMessage ChannelTemplate_ResolvedWithFallback(string system, string? templateName)
    {
        templateName ??= "Unnamed";

        return new LogMessage(1102, system, $"Cannot find named channel template '{templateName}', falling back to primary.")
        {
            FormatText = "Cannot find named template '{templateName}', falling back to primary.",
            FormatArgs = new[] { templateName },
            Reason = $"Cannot find named channel template '{templateName}', falling back to primary."
        };
    }

    public static LogMessage Events_NoTopic(string system)
    {
        return new LogMessage(1200, system, "Event has not topic.")
        {
            FormatText = "Event has no topic.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_TooOld(string system)
    {
        return new LogMessage(1200, system, "Event is too old and will be skipped.")
        {
            FormatText = "Event is too old and will be skipped.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_NoTemplateSubject(string system, string templateName)
    {
        return new LogMessage(1200, system, $"Template '{templateName}' has no subject.")
        {
            FormatText = "Template '{templateName}' has no subject.",
            FormatArgs = new[] { templateName },
            Reason = null
        };
    }

    public static LogMessage Events_AlreadyProcessed(string system)
    {
        return new LogMessage(1200, system, "Event with this id has already been processed.")
        {
            FormatText = "Event with this id has already been processed.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_NoSubscriber(string system)
    {
        return new LogMessage(1200, system, "Event has no subscriber.")
        {
            FormatText = "Event has no subscriber.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_CreationFailed(string system)
    {
        return new LogMessage(1200, system, "Failed to create event.")
        {
            FormatText = "Event with this id has already been processed.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Events_NoSubjectOrTemplateCode(string system)
    {
        return new LogMessage(1200, system, "Event with this id has already been processed.")
        {
            FormatText = "Event has neither a subject nor a template code.",
            FormatArgs = null,
            Reason = null
        };
    }

    public static LogMessage Integration_Removed(string system)
    {
        return new LogMessage(1103, system, $"Integration has been removed and is not available anymore.")
        {
            FormatText = "Integration has been removed and is not available anymore.",
            FormatArgs = null,
            Reason = "Integration removed."
        };
    }

    public static LogMessage User_Deleted(string system, string userId)
    {
        return new LogMessage(2200, system, "User has been deleted")
        {
            FormatText = "User '{userId}' has been deleted.",
            FormatArgs = new[] { userId },
            Reason = null,
        };
    }

    public static LogMessage User_EmailRemoved(string system, string userId)
    {
        return new LogMessage(2201, system, "User has removed the email address.")
        {
            FormatText = "User '{userId}' has removed the email address.",
            FormatArgs = new[] { userId },
            Reason = "Email address removed.",
        };
    }

    public static LogMessage User_LanguageNotValid(string system, string userId, string language, string fallback)
    {
        return new LogMessage(2202, system, $"User has unsupported language '{language}', using '{fallback}' instead.")
        {
            FormatText = "User '{userId}' has unsupported language '{language}', using '{fallback}' instead.",
            FormatArgs = new[] { userId, language, fallback },
            Reason = "Email address removed.",
        };
    }

    public static LogMessage MobilePush_TokenRemoved(string system, string userId, string token)
    {
        return new LogMessage(3000, system, "Token has been removed.")
        {
            FormatText = "Token '{token}' has been removed for user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = "Token removed."
        };
    }

    public static LogMessage WebPush_TokenRemoved(string system, string userId, string token)
    {
        return new LogMessage(4000, system, "Token has been removed.")
        {
            FormatText = "Token '{token}' has been removed for user '{userId}'.",
            FormatArgs = new[] { token, userId },
            Reason = "Token removed."
        };
    }

    public static LogMessage Messaging_CallbackError(string system, string? details)
    {
        details ??= "NoDetails";

        return new LogMessage(5000, system, $"Callback failed with '{details}'.")
        {
            FormatText = "Callback failed with '{details}'.",
            FormatArgs = new[] { details },
            Reason = "Callback failed."
        };
    }

    public static LogMessage Sms_CallbackError(string system, string? details)
    {
        details ??= "NoDetails";

        return new LogMessage(6000, system, $"Callback failed with '{details}'.")
        {
            FormatText = "Callback failed with '{details}'.",
            FormatArgs = new[] { details },
            Reason = "Callback failed."
        };
    }
}
