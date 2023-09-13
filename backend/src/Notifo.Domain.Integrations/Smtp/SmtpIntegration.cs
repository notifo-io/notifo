// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Smtp;

public sealed partial class SmtpIntegration : IIntegration
{
    private readonly SmtpEmailServerPool serverPool;

    public static readonly IntegrationProperty HostProperty = new IntegrationProperty("host", PropertyType.Text)
    {
        EditorLabel = Texts.SMTP_HostLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public static readonly IntegrationProperty HostPortProperty = new IntegrationProperty("port", PropertyType.Number)
    {
        EditorLabel = Texts.SMTP_PortLabel,
        EditorDescription = null,
        DefaultValue = "587"
    };

    public static readonly IntegrationProperty UsernameProperty = new IntegrationProperty("username", PropertyType.Text)
    {
        EditorLabel = Texts.SMTP_UsernameLabel,
        EditorDescription = Texts.SMTP_UsernameHints
    };

    public static readonly IntegrationProperty PasswordProperty = new IntegrationProperty("password", PropertyType.Password)
    {
        EditorLabel = Texts.SMTP_PasswordLabel,
        EditorDescription = Texts.SMTP_PasswordHints
    };

    public static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", PropertyType.Text)
    {
        Pattern = Patterns.Email,
        EditorLabel = Texts.Email_FromEmailLabel,
        EditorDescription = Texts.Email_FromEmailDescription,
        IsRequired = true,
        Summary = true
    };

    public static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", PropertyType.Text)
    {
        EditorLabel = Texts.Email_FromNameLabel,
        EditorDescription = Texts.Email_FromNameDescription,
        IsRequired = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "SMTP",
            Texts.SMTP_Name,
            "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512' xml:space='preserve'><linearGradient id='a' gradientUnits='userSpaceOnUse' x1='256' y1='347.889' x2='256' y2='0'><stop offset='0' style='stop-color:#cf993f'/><stop offset='.768' style='stop-color:#dfa34f'/></linearGradient><path style='fill:url(#a)' d='M511.972 170.467v11.507c-.6.427-1.372.937-2.143 1.448l-19.117 13.041c-1.285.853-2.828 1.875-4.371 2.984-7.973 5.283-18.345 12.358-25.203 16.876L293.376 329.769l-5.315 3.58-15.602 10.569c-1.972 1.278-4.286 2.301-6.858 2.983-1.029.34-2.229.511-3.343.681-.172.085-.343.085-.428.085-1.972.256-3.943.256-5.83.171-1.886.085-3.857.085-5.829-.171a27.086 27.086 0 0 1-3.772-.767c-2.572-.681-4.886-1.705-6.858-2.983l-7.029-4.774-8.572-5.795L50.862 216.324a276.656 276.656 0 0 0-6.429-4.262c-6-4.006-13.03-8.779-18.774-12.614-1.543-1.109-3.086-2.131-4.372-2.984L2.171 183.423c-.771-.511-1.543-1.022-2.143-1.448v-11.507c0-1.96.257-3.494.686-4.943 0-.084.086-.084.086-.084.429-.341.857-.597 1.371-.939l6.172-4.262 12.944-8.779c8.143-5.454 21.431-14.489 29.574-19.859L239.541 4.007C244.085 1.109 250.085-.255 256 .086c.6 0 1.114-.085 1.715-.085h.086c5.315 0 10.63 1.365 14.659 4.007l188.679 127.594c6.772 4.432 17.059 11.421 25.031 16.792 1.629 1.107 3.172 2.131 4.543 3.067l12.944 8.779 6.172 4.262c.514.341.943.597 1.371.939.515 1.447.772 3.066.772 5.026z'/><linearGradient id='b' gradientUnits='userSpaceOnUse' x1='256' y1='398.917' x2='256' y2='51.064'><stop offset='0' style='stop-color:#cf993f'/><stop offset='.768' style='stop-color:#dfa34f'/></linearGradient><path style='fill:url(#b)' d='M512 221.49v11.441c-.625.443-1.429.976-2.141 1.508l-19.179 13.037c-8.116 5.41-21.407 14.455-29.524 19.866L272.502 394.875c-2.051 1.33-4.371 2.305-6.868 3.015-3.032.887-6.422 1.153-9.633.975-3.211.178-6.601-.089-9.633-.975-2.498-.71-4.817-1.685-6.868-3.015L50.845 267.341c-8.117-5.411-21.408-14.456-29.524-19.866L2.142 234.439c-.712-.533-1.516-1.065-2.141-1.508V221.49c0-1.951.268-3.548.714-4.966 0-.089.089-.089.089-.089.446-.355.892-.622 1.337-.977l6.245-4.257 12.934-8.78c8.116-5.41 21.407-14.455 29.524-19.866L239.499 55.023c4.549-2.927 10.614-4.258 16.501-3.903 5.887-.355 12.042.975 16.501 3.903l188.655 127.532c8.117 5.411 21.408 14.456 29.524 19.866l12.934 8.78 6.245 4.257c.445.355.891.621 1.337.977.536 1.418.804 3.104.804 5.055z'/><path style='fill:#c0874a' d='M262.258 314.513v35.968h-1.029l-11.487.767H.028V173.792c0-1.96.257-3.58.686-4.944 0-.085.086-.085.086-.085.943-2.983 2.829-4.688 5.315-5.114l10.716 6.307 5.057 2.983 23.06 13.553 1.2.681 14.659 8.609 166.819 98.359.343.171 33.089 19.433 1.2.768z'/><path style='fill:#f0ba7d' d='M266.715 336.247c0 .085-1.714 1.279-4.457 3.239a177.152 177.152 0 0 0-6.258 4.262c-1.2.767-2.486 1.619-3.857 2.557l-1.972 1.363-3.772 2.557-1.629 1.023-26.66 17.984c-2.829 1.875-5.658 3.835-8.229 5.626-4.886 3.324-8.916 6.051-10.287 6.903-1.972 1.449-10.201 6.99-18.345 12.529l-44.577 30.088-.172.171-.085.085-109.384 73.813c-1.886 1.279-3.943 2.642-5.915 4.006-1.115.767-2.229 1.449-3.343 2.216H8.515c-4.972 0-7.801-1.108-8.401-2.983v-.085c0-.341-.086-.597 0-1.023-.086-.512-.086-1.023-.086-1.62V173.792c0-1.96.257-3.58.686-4.944 0-.085.086-.085.086-.085.943-2.983 2.829-4.688 5.315-5.114.771-.256 1.457-.256 2.229-.171 1.886.085 4.029.938 6.344 2.472l30.346 20.456 1.114.767 13.116 8.864.771.512c8.058 5.455 20.745 14.064 28.632 19.348l153.446 103.729c.172.085.343.171.429.256l.771.512c2.057 1.363 4.029 2.728 5.829 3.92 3.001 1.96 5.401 3.665 6.858 4.688.857.512 1.458.852 1.543.937.343.255 2.486 1.705 4.715 3.324.086 0 .086.085.171.085 2.315 1.62 4.286 2.813 4.286 2.899z'/><path style='fill:#e5a864' d='M511.865 171.974c.074 1 .134 2.92.134 4.267v323.257c0 .317-.089 1.248-.089 1.419V501.627c0 .024-.783.723-1.74 1.551-.957.829-7.747 1.507-9.094 1.507h-4.378c-1.347 0-3.365-.612-4.486-1.359l-5.204-3.489c-1.12-.748-2.95-1.976-4.066-2.731L332.776 395.641l-4.058-2.742-14.324-9.753a1373.46 1373.46 0 0 0-4.05-2.754l-14.412-9.754a1252.76 1252.76 0 0 0-4.055-2.747l-24.216-16.33-4.062-2.736-1.734-1.167a847.668 847.668 0 0 0-4.072-2.72s-.635-.421-1.795-1.219c-6.155-4.257-10.704-7.362-10.704-7.451 0-.089 1.963-1.329 4.282-2.926 2.318-1.597 2.885-1.986 2.885-1.986a337.009 337.009 0 0 0 2.332-1.607c.172-.122 2.109-1.383 3.214-2.152 0 0 5.125-3.567 10.657-7.204l.268-.178.668-.443c.073-.049.954-.617 1.74-1.153.785-.536 2.341-1.592 3.457-2.346l146.508-98.984a3606.7 3606.7 0 0 0 4.056-2.745l25.377-17.123a5759.21 5759.21 0 0 0 4.06-2.739l40.451-27.324c1.116-.754 3-1.892 4.187-2.529 0 0 1.065-.637 2.169-.881s3.079-.189 4.389.123l3.792 2.662c.776 1.103 2.005 4.241 2.079 5.241z'/><path style='fill:#c0874a' d='M511.999 458.657v40.265c0 .62 0 1.153-.089 1.685.089.355 0 .621 0 .976v.089c-.625 1.862-3.48 3.014-8.385 3.014H46.383l90.001-80.084.089-.089L249.13 324.297l10.972-9.668 2.942-2.661 9.723 5.677 239.232 141.012z'/><path style='fill:#ecb168' d='M511.999 497.414v1.508c0 .62 0 1.153-.089 1.685.089.355 0 .621 0 .976v.089c-.625 1.862-3.48 3.014-8.385 3.014H8.474c-4.905 0-7.76-1.152-8.385-3.014v-.089c0-.355-.089-.621 0-.976C0 500.075 0 499.542 0 498.922v-1.508c.803-1.064 1.963-2.128 3.479-3.193l238.07-173.651c.089 0 .089-.088.179-.176l.803-.532c4.905-3.46 9.901-5.855 13.469-6.74 1.249.354 2.586.797 4.104 1.507 2.854 1.153 6.154 3.016 9.365 5.233.356.265.624.443.981.708l238.069 173.651c1.516 1.153 2.676 2.217 3.48 3.193z'/></svg>",
            new List<IntegrationProperty>
            {
                HostProperty,
                HostPortProperty,
                UsernameProperty,
                PasswordProperty,
                FromEmailProperty,
                FromNameProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Email
            })
        {
            Description = Texts.SMTP_Description
        };

    public SmtpIntegration(SmtpEmailServerPool serverPool)
    {
        this.serverPool = serverPool;
    }
}
