// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Domain.Liquid;

public abstract class LiquidNotificationBase
{
    private readonly NotificationFormatting<string> formatting;
    private readonly IImageFormatter imageFormatter;
    private readonly string imagePresetSmall;
    private readonly string imagePresetLarge;
    private string? imageLarge;
    private string? imageSmall;

    public string Subject => formatting.Subject;

    public string? Body => formatting.Body.OrNull();

    public string? LinkUrl => formatting.LinkUrl.OrNull();

    public string? LinkText => formatting.LinkText.OrNull();

    public string? ImageSmall
    {
        get => imageSmall ??= imageFormatter.AddPreset(formatting.ImageSmall, imagePresetSmall);
    }

    public string? ImageLarge
    {
        get => imageLarge ??= imageFormatter.AddPreset(formatting.ImageSmall, imagePresetLarge);
    }

    protected LiquidNotificationBase(
        NotificationFormatting<string> formatting,
        string imagePresetSmall,
        string imagePresetLarge,
        IImageFormatter imageFormatter)
    {
        this.imageFormatter = imageFormatter;
        this.imagePresetSmall = imagePresetSmall;
        this.imagePresetLarge = imagePresetLarge;
        this.formatting = formatting;
    }

    protected static void DescribeBase(LiquidProperties properties)
    {
        properties.AddString("subject",
            "The notification subject. Cannot be null or undefined.");

        properties.AddString("body",
            "The notification body. Can be null or undefined.");

        properties.AddString("linkUrl",
            "The link URL. Can be null or undefined.");

        properties.AddString("linkText",
            "The link text that can be set when a linkUrl is set. Can be null or undefined.");

        properties.AddString("imageSmall",
            "The URL to the small image. Optimized for the current use case (e.g. emails). Can be null or undefined.");

        properties.AddString("imageLarge",
            "The URL to the large image. Optimized for the current use case (e.g. emails). Can be null or undefined.");
    }
}
