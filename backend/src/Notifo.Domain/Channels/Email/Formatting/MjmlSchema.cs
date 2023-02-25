// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Mjml.Net;
using Mjml.Net.Components;
using Mjml.Net.Types;
using Notifo.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain.Channels.Email.Formatting;

public sealed class MjmlSchema
{
    [JsonPropertyName("!top")]
    public List<string> Top { get; } = new List<string>();

    [JsonIgnore]
    public Dictionary<string, MjmlSchemaElement> Elements { get; } = new Dictionary<string, MjmlSchemaElement>();

    [JsonExtensionData]
    public Dictionary<string, object> ExtraData
    {
        get => Elements.ToDictionary(x => x.Key, x => (object)x.Value);
    }

    public static MjmlSchema Build(IMjmlRenderer renderer)
    {
        var result = new MjmlSchema();

        var componentList = renderer.Components.Select(x => x()).Where(x => x is not IncludeComponent).ToList();
        var componentChilds = new Dictionary<string, List<string>>();

        foreach (var component in componentList)
        {
            if (component.AllowedParents != null)
            {
                foreach (var parent in component.AllowedParents)
                {
                    componentChilds.GetOrAddNew(parent).Add(component.ComponentName);
                }
            }
        }

        foreach (var component in componentList)
        {
            if (component.AllowedParents == null)
            {
                result.Top.Add(component.ComponentName);
            }

            var element = new MjmlSchemaElement
            {
                Children = componentChilds.GetOrAddDefault(component.ComponentName)
            };

            if (component.AllowedFields != null)
            {
                foreach (var (fieldName, type) in component.AllowedFields)
                {
                    var allowedValues = (type as EnumType)?.AllowedValues.ToList();

                    element.Attributes[fieldName] = allowedValues;
                }
            }

            result.Elements[component.ComponentName] = element;
        }

        return result;
    }
}

public sealed class MjmlSchemaElement
{
    [JsonPropertyName("attrs")]
    public Dictionary<string, List<string>?> Attributes { get; } = new Dictionary<string, List<string>?>();

    [JsonPropertyName("children")]
    public List<string>? Children { get; set; }
}
