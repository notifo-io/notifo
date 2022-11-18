// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Utils;

public class TemplateCacheTests
{
    [Fact]
    public void Should_cache_template()
    {
        var template1 = TemplateCache.Parse("{{ user.name }}");
        var template2 = TemplateCache.Parse("{{ user.name }}");

        Assert.Null(template1.Error);
        Assert.Null(template2.Error);
        Assert.NotNull(template1.Template);
        Assert.NotNull(template2.Template);
        Assert.Same(template1.Template, template2.Template);
    }

    [Fact]
    public void Should_not_cache_template_if_bypassed()
    {
        var template1 = TemplateCache.Parse("{{ user.name }}", true);
        var template2 = TemplateCache.Parse("{{ user.name }}", true);

        Assert.NotSame(template1.Template, template2.Template);
    }

    [Fact]
    public void Should_cache_error()
    {
        var template1 = TemplateCache.Parse("{% if user.name %}");
        var template2 = TemplateCache.Parse("{% if user.name %}");

        Assert.NotNull(template1.Error);
        Assert.NotNull(template2.Error);
        Assert.Null(template1.Template);
        Assert.Null(template2.Template);
        Assert.Same(template1.Error, template2.Error);
    }

    [Fact]
    public void Should_not_cache_error_if_bypassed()
    {
        var template1 = TemplateCache.Parse("{% if user.name %}", true);
        var template2 = TemplateCache.Parse("{% if user.name %}", true);

        Assert.NotSame(template1.Error, template2.Error);
    }
}
