// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace Notifo.Identity.Shared;

public abstract class XmlRepositoryTests
{
    private readonly string friendlyName = Guid.NewGuid().ToString();

    protected abstract Task<IXmlRepository> CreateSutAsync();

    [Fact]
    public async Task Should_store_and_get_element()
    {
        var sut = await CreateSutAsync();

        var element = CreateElement(friendlyName, "value");

        sut.StoreElement(element, friendlyName);

        var result = sut.GetAllElements();

        Assert.Contains(result, x => XNode.DeepEquals(x, element));
    }

    [Fact]
    public async Task Should_get_all_stored_elements()
    {
        var sut = await CreateSutAsync();

        var otherName = Guid.NewGuid().ToString();

        var element1 = CreateElement(friendlyName, "value1");
        var element2 = CreateElement(otherName, "value2");

        sut.StoreElement(element1, friendlyName);
        sut.StoreElement(element2, otherName);

        var result = sut.GetAllElements();

        Assert.Contains(result, x => XNode.DeepEquals(x, element1));
        Assert.Contains(result, x => XNode.DeepEquals(x, element2));
    }

    [Fact]
    public async Task Should_replace_element_with_same_friendly_name()
    {
        var sut = await CreateSutAsync();

        sut.StoreElement(CreateElement(friendlyName, "value1"), friendlyName);
        sut.StoreElement(CreateElement(friendlyName, "value2"), friendlyName);

        var result = sut.GetAllElements().Where(x => x.Attribute("name")?.Value == friendlyName);

        Assert.Equal("value2", Assert.Single(result).Value);
    }

    [Fact]
    public async Task Should_get_stored_elements_from_other_instance()
    {
        var sut1 = await CreateSutAsync();
        var sut2 = await CreateSutAsync();

        var element = CreateElement(friendlyName, "value");

        sut1.StoreElement(element, friendlyName);

        var result = sut2.GetAllElements();

        Assert.Contains(result, x => XNode.DeepEquals(x, element));
    }

    private static XElement CreateElement(string name, string value)
    {
        return new XElement("key",
            new XAttribute("name", name),
            new XElement("value", value));
    }
}
