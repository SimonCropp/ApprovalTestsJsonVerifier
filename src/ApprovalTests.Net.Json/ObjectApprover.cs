﻿using System;
using ApprovalTests;
using Newtonsoft.Json;
using ObjectApproval;

public static partial class ObjectApprover
{
    public static void Verify(object? target)
    {
        Verify(target, null);
    }

    public static void Verify(object? target, Func<string, string>? scrubber = null)
    {
        Verify(target, scrubber, null);
    }

    public static void Verify(object? target, Func<string, string>? scrubber = null, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        var formatJson = AsFormattedJson(target, jsonSerializerSettings);
        if (scrubber == null)
        {
            scrubber = s => s;
        }

        Approvals.Verify(formatJson, scrubber);
    }

    public static void Verify(
        object? target,
        bool ignoreEmptyCollections = true,
        bool scrubGuids = true,
        bool scrubDateTimes = true,
        bool ignoreFalse = true,
        Func<string, string>? scrubber = null)
    {
        var settings = SerializerBuilder.BuildSettings(ignoreEmptyCollections, scrubGuids, scrubDateTimes, ignoreFalse);
        var json = AsFormattedJson(target, settings);
        if (scrubber != null)
        {
            json = scrubber(json);
        }

        Approvals.Verify(json);
    }

    static Action<string> verifyAction = Approvals.Verify;

    public static void SetVerifyAction(Action<string> verify)
    {
        Guard.AgainstNull(verify, nameof(verify));
        verifyAction = verify;
    }
}
