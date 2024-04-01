// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain.Liquid;

public sealed class LiquidProperties : List<LiquidProperty>
{
    private readonly Stack<string> pathStack = new Stack<string>();

    public void AddArray(string path, string? description = null)
    {
        Add(new LiquidProperty(FullPath(path), LiquidPropertyType.Array, description));
    }

    public void AddString(string path, string? description = null)
    {
        Add(new LiquidProperty(FullPath(path), LiquidPropertyType.String, description));
    }

    public void AddNumber(string path, string? description = null)
    {
        Add(new LiquidProperty(FullPath(path), LiquidPropertyType.Number, description));
    }

    public void AddBoolean(string path, string? description = null)
    {
        Add(new LiquidProperty(FullPath(path), LiquidPropertyType.Boolean, description));
    }

    public void AddObject(string path, Action inner, string? description = null)
    {
        pathStack.Push(path);

        Add(new LiquidProperty(FullPath(path), LiquidPropertyType.Object, description));
        inner();

        pathStack.Pop();
    }

    private string FullPath(string path)
    {
        if (pathStack.Count > 0)
        {
            path = $"{string.Join('.', pathStack)}.{path}";
        }

        return path;
    }
}

public sealed record LiquidProperty(string Path, LiquidPropertyType Type, string? Description)
{
}

public enum LiquidPropertyType
{
    Array,
    String,
    Number,
    Boolean,
    Object
}
