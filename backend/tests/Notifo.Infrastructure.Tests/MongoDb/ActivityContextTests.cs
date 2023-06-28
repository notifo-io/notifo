// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;

namespace Notifo.Infrastructure.MongoDB;

public class ActivityContextTests
{
    [Fact]
    public void Should_serialize_and_deserialize_trace_id()
    {
        var sut = ActivityTraceId.CreateRandom();

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_default_trace_id()
    {
        var sut = default(ActivityTraceId);

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_span_id()
    {
        var sut = ActivitySpanId.CreateRandom();

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_default_span_id()
    {
        var sut = default(ActivitySpanId);

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var sut =
            new ActivityContext(
                ActivityTraceId.CreateRandom(),
                ActivitySpanId.CreateRandom(),
                ActivityTraceFlags.Recorded,
                "State",
                true);

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_with_null_state()
    {
        var sut =
            new ActivityContext(
                ActivityTraceId.CreateRandom(),
                ActivitySpanId.CreateRandom(),
                ActivityTraceFlags.Recorded,
                null,
                true);

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_with_default_ids()
    {
        var sut =
            new ActivityContext(
                default,
                default,
                ActivityTraceFlags.Recorded,
                "State",
                true);

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_default()
    {
        var sut = default(ActivityContext);

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }
}
