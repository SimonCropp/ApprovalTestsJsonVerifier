<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# <img src="https://raw.github.com/SimonCropp/ObjectApproval/master/icon.png" height="40px"> ObjectApproval

Extends [ApprovalTests](https://github.com/approvals/ApprovalTests.Net) to allow simple approval of complex models using [Json.net](https://www.newtonsoft.com/json).


## The NuGet package [![NuGet Status](http://img.shields.io/nuget/v/ObjectApproval.svg)](https://www.nuget.org/packages/ObjectApproval/)

https://nuget.org/packages/ObjectApproval/

<!-- toc -->
## Contents

  * [Usage](#usage)
    * [Validating multiple instances](#validating-multiple-instances)
  * [Serializer settings](#serializer-settings)
    * [Default settings](#default-settings)
    * [Single quotes used](#single-quotes-used)
    * [QuoteName is false](#quotename-is-false)
    * [Empty collections are ignored](#empty-collections-are-ignored)
    * [Guids are scrubbed](#guids-are-scrubbed)
    * [Dates are scrubbed](#dates-are-scrubbed)
    * [Default Booleans are ignored](#default-booleans-are-ignored)
    * [Change defaults at the verification level](#change-defaults-at-the-verification-level)
    * [Changing settings globally](#changing-settings-globally)
    * [Scoped settings](#scoped-settings)
    * [Ignoring a type](#ignoring-a-type)
    * [Ignoring a instance](#ignoring-a-instance)
    * [Ignore member by expressions](#ignore-member-by-expressions)
    * [Ignore member by name](#ignore-member-by-name)
    * [Members that throw](#members-that-throw)
    * [Scrubber](#scrubber)
  * [Named Tuples](#named-tuples)
<!-- endtoc -->



## Usage

Assuming you have previously verified and approved using this.

<!-- snippet: before -->
```cs
var person = new Person
{
    GivenNames = "John",
    FamilyName = "Smith",
    Spouse = "Jill",
    Address = new Address
    {
        Street = "1 Puddle Lane",
        Country = "USA"
    }
};

ObjectApprover.Verify(person);
```
<sup>[snippet source](/src/Tests/Samples.cs#L64-L80)</sup>
<!-- endsnippet -->

Then you attempt to verify this 

<!-- snippet: after -->
```cs
var person = new Person
{
    GivenNames = "John",
    FamilyName = "Smith",
    Spouse = "Jill",
    Address = new Address
    {
        Street = "1 Puddle Lane",
        Suburb = "Gotham",
        Country = "USA"
    }
};

ObjectApprover.Verify(person);
```
<sup>[snippet source](/src/Tests/Samples.cs#L125-L142)</sup>
<!-- endsnippet -->

The serialized json version of these will then be compared and you will be displayed the differences in the diff tool you have asked ApprovalTests to use. For example:

![SampleDiff](https://raw.github.com/SimonCropp/ObjectApproval/master/src/SampleDiff.png)

Note that the output is technically not valid json. [Single quotes are used](#single-quotes-used) and [names are not quoted](#quotename-is-false). The reason for this is to make the resulting output easier to read and understand.


### Validating multiple instances

When validating multiple instances, an [anonymous type](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/anonymous-types) can be used for verification

<!-- snippet: anon -->
```cs
var person1 = new Person
{
    GivenNames = "John",
    FamilyName = "Smith"
};
var person2 = new Person
{
    GivenNames = "Marianne",
    FamilyName = "Aguirre"
};

ObjectApprover.Verify(
    new
    {
        person1,
        person2
    });
```
<sup>[snippet source](/src/Tests/Samples.cs#L86-L106)</sup>
<!-- endsnippet -->

Results in the following:

<!-- snippet: Samples.Anon.approved.txt -->
```txt
{
  person1: {
    GivenNames: 'John',
    FamilyName: 'Smith'
  },
  person2: {
    GivenNames: 'Marianne',
    FamilyName: 'Aguirre'
  }
}
```
<sup>[snippet source](/src/Tests/Samples.Anon.approved.txt#L1-L10)</sup>
<!-- endsnippet -->


## Serializer settings

`SerializerBuilder` is used to build the Json.net `JsonSerializerSettings`. This is done for every verification run by calling `SerializerBuilder.BuildSettings()`.

All modifications of `SerializerBuilder` behavior is global for all verifications and should be done once at assembly load time.


### Default settings

The default serialization settings are:

<!-- snippet: defaultSerialization -->
```cs
var settings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore
};
```
<sup>[snippet source](/src/ObjectApproval/Helpers/SerializerBuilder.cs#L145-L154)</sup>
<!-- endsnippet -->


### Single quotes used

[JsonTextWriter.QuoteChar](https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_JsonTextWriter_QuoteChar.htm) is set to single quotes `'`. The reason for this is that it makes approval files cleaner and easier to read and visualize/understand differences


### QuoteName is false

[JsonTextWriter.QuoteName](https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_JsonTextWriter_QuoteName.htm) is set to false. The reason for this is that it makes approval files cleaner and easier to read and visualize/understand differences


### Empty collections are ignored

By default empty collections are ignored during verification.

To disable this behavior use:

```cs
SerializerBuilder.IgnoreEmptyCollections = false;
```


### Guids are scrubbed

By default guids are sanitized during verification. This is done by finding each guid and taking a counter based that that specific guid. That counter is then used replace the guid values. This allows for repeatable tests when guid values are changing.

<!-- snippet: guid -->
```cs
var guid = Guid.NewGuid();
var target = new GuidTarget
{
    Guid = guid,
    GuidNullable = guid,
    GuidString = guid.ToString(),
    OtherGuid = Guid.NewGuid(),
};

ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L21-L34)</sup>
<!-- endsnippet -->

Results in the following:

<!-- snippet: ObjectApproverTests.ShouldReUseGuid.approved.txt -->
```txt
{
  Guid: Guid_1,
  GuidNullable: Guid_1,
  GuidString: Guid_1,
  OtherGuid: Guid_2
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.ShouldReUseGuid.approved.txt#L1-L6)</sup>
<!-- endsnippet -->

To disable this behavior use:

```cs
SerializerBuilder.ScrubGuids = false;
```


### Dates are scrubbed

By default dates (`DateTime` and `DateTimeOffset`) are sanitized during verification. This is done by finding each date and taking a counter based that that specific date. That counter is then used replace the date values. This allows for repeatable tests when date values are changing.

<!-- snippet: Date -->
```cs
var dateTime = DateTime.Now;
var dateTimeOffset = DateTimeOffset.Now;
var target = new DateTimeTarget
{
    DateTime = dateTime,
    DateTimeNullable = dateTime,
    DateTimeString = dateTime.ToString("F"),
    DateTimeOffset = dateTimeOffset,
    DateTimeOffsetNullable = dateTimeOffset,
    DateTimeOffsetString = dateTimeOffset.ToString("F"),
};

ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L517-L533)</sup>
<!-- endsnippet -->

Results in the following:

<!-- snippet: ObjectApproverTests.ShouldReUseDatetime.approved.txt -->
```txt
{
  DateTime: DateTime_1,
  DateTimeNullable: DateTime_1,
  DateTimeOffset: DateTimeOffset_1,
  DateTimeOffsetNullable: DateTimeOffset_1,
  DateTimeString: DateTimeOffset_2,
  DateTimeOffsetString: DateTimeOffset_2
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.ShouldReUseDatetime.approved.txt#L1-L8)</sup>
<!-- endsnippet -->

To disable this behavior use:

```cs
SerializerBuilder.ScrubDateTimes = false;
```


### Default Booleans are ignored

By default values of `bool` and `bool?` are ignored during verification. So properties that equate to 'false' will not be written,

To disable this behavior use:

```cs
SerializerBuilder.IgnoreFalse = false;
```


### Change defaults at the verification level

`DateTime`, `DateTimeOffset`, `Guid`, `bool`, and empty collection behavior can also be controlled at the verification level: 

<!-- snippet: ChangeDefaultsPerVerification -->
```cs
ObjectApprover.Verify(target,
    ignoreEmptyCollections: false,
    scrubGuids: false,
    scrubDateTimes: false,
    ignoreFalse: false);
```
<sup>[snippet source](/src/Tests/Samples.cs#L28-L36)</sup>
<!-- endsnippet -->


### Changing settings globally

To change the serialization settings for all verifications use `SerializerBuilder.ExtraSettings`:

<!-- snippet: ExtraSettings -->
```cs
SerializerBuilder.ExtraSettings =
    jsonSerializerSettings =>
    {
        jsonSerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
        jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
    };
```
<sup>[snippet source](/src/Tests/Samples.cs#L111-L120)</sup>
<!-- endsnippet -->


### Scoped settings

<!-- snippet: ScopedSerializer -->
```cs
var person = new Person
{
    GivenNames = "John",
    FamilyName = "Smith",
    Dob = new DateTime(2000, 10, 1),
};
var serializerSettings = SerializerBuilder.BuildSettings(scrubDateTimes: false);
serializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
ObjectApprover.Verify(person, jsonSerializerSettings: serializerSettings);
```
<sup>[snippet source](/src/Tests/Samples.cs#L47-L59)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: Samples.ScopedSerializer.approved.txt -->
```txt
{
  GivenNames: 'John',
  FamilyName: 'Smith',
  Dob: '\/Date(970322400000+1000)\/'
}
```
<sup>[snippet source](/src/Tests/Samples.ScopedSerializer.approved.txt#L1-L5)</sup>
<!-- endsnippet -->


### Ignoring a type

To ignore all members that match a certain type:

<!-- snippet: AddIgnoreType -->
```cs
// Done on static startup
SerializerBuilder.IgnoreMembersWithType<ToIgnore>();

// Done as part of test
var target = new IgnoreTypeTarget
{
    ToIgnore = new ToIgnore
    {
        Property = "Value"
    },
    ToInclude = new ToInclude
    {
        Property = "Value"
    }
};
ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L150-L169)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: ObjectApproverTests.IgnoreType.approved.txt -->
```txt
{
  ToInclude: {
    Property: 'Value'
  }
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.IgnoreType.approved.txt#L1-L5)</sup>
<!-- endsnippet -->


### Ignoring a instance

To ignore instances of a type based on delegate:

<!-- snippet: AddIgnoreInstance -->
```cs
// Done on static startup
SerializerBuilder.IgnoreInstance<Instance>(x => x.Property == "Ignore");

// Done as part of test
var target = new IgnoreInstanceTarget
{
    ToIgnore = new Instance
    {
        Property = "Ignore"
    },
    ToInclude = new Instance
    {
        Property = "Include"
    },
};
ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L92-L111)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: ObjectApproverTests.IgnoreInstance.approved.txt -->
```txt
{
  ToInclude: {
    Property: 'Include'
  }
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.IgnoreInstance.approved.txt#L1-L5)</sup>
<!-- endsnippet -->


### Ignore member by expressions

To ignore members of a certain type using an expression:

<!-- snippet: IgnoreMemberByExpression -->
```cs
// Done on static startup
SerializerBuilder.IgnoreMember<IgnoreExplicitTarget>(x => x.Property);
SerializerBuilder.IgnoreMember<IgnoreExplicitTarget>(x => x.Field);
SerializerBuilder.IgnoreMember<IgnoreExplicitTarget>(x => x.GetOnlyProperty);
SerializerBuilder.IgnoreMember<IgnoreExplicitTarget>(x => x.PropertyThatThrows);

// Done as part of test
var target = new IgnoreExplicitTarget
{
    Include = "Value",
    Field = "Value",
    Property = "Value"
};
ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L209-L226)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: ObjectApproverTests.IgnoreMemberByExpression.approved.txt -->
```txt
{
  Include: 'Value'
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.IgnoreMemberByExpression.approved.txt#L1-L3)</sup>
<!-- endsnippet -->


### Ignore member by name

To ignore members of a certain type using type and name:

<!-- snippet: IgnoreMemberByName -->
```cs
// Done on static startup
var type = typeof(IgnoreExplicitTarget);
SerializerBuilder.IgnoreMember(type, "Property");
SerializerBuilder.IgnoreMember(type, "Field");
SerializerBuilder.IgnoreMember(type, "GetOnlyProperty");
SerializerBuilder.IgnoreMember(type, "PropertyThatThrows");

// Done as part of test
var target = new IgnoreExplicitTarget
{
    Include = "Value",
    Field = "Value",
    Property = "Value"
};
ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L232-L250)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: ObjectApproverTests.IgnoreMemberByName.approved.txt -->
```txt
{
  Include: 'Value'
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.IgnoreMemberByName.approved.txt#L1-L3)</sup>
<!-- endsnippet -->


### Members that throw

Members that throw exceptions can be excluded from serialization based on the exception type or properties.

By default members that throw `NotImplementedException` or `NotSupportedException` are ignored.

Note that this is global for all members on all types.

Ignore by exception type:

<!-- snippet: IgnoreMembersThatThrow -->
```cs
// Done on static startup
SerializerBuilder.IgnoreMembersThatThrow<CustomException>();

// Done as part of test
var target = new WithCustomException();
ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L295-L304)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: ObjectApproverTests.CustomExceptionProp.approved.txt -->
```txt
{}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.CustomExceptionProp.approved.txt#L1-L1)</sup>
<!-- endsnippet -->

Ignore by exception type and expression:

<!-- snippet: IgnoreMembersThatThrowExpression -->
```cs
// Done on static startup
SerializerBuilder.IgnoreMembersThatThrow<Exception>(
    x => { return x.Message == "Ignore"; });

// Done as part of test
var target = new WithExceptionIgnoreMessage();
ObjectApprover.Verify(target);
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L359-L369)</sup>
<!-- endsnippet -->

Result:

<!-- snippet: ObjectApproverTests.ExceptionMessageProp.approved.txt -->
```txt
{}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.ExceptionMessageProp.approved.txt#L1-L1)</sup>
<!-- endsnippet -->


### Scrubber

A scrubber can be used to cleanup or sanitize the resultant serialized string prior to verification.

<!-- snippet: Scrubber -->
```cs
var target = new ToBeScrubbed
{
    RowVersion = "0x00000000000007D3"
};

ObjectApprover.Verify(target,
    scrubber: s => s.Replace("0x00000000000007D3", "TheRowVersion"));
```
<sup>[snippet source](/src/Tests/Samples.cs#L13-L23)</sup>
<!-- endsnippet -->

Results in the following:

```graphql
{
  RowVersion: 'ThRowVersion'
}
```

## Named Tuples

Instances of [named tuples](https://docs.microsoft.com/en-us/dotnet/csharp/tuples#named-and-unnamed-tuples) can be verified using `ObjectApprover.VerifyTuple`.

Due to the use of [ITuple](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.ituple), this approach is only available an net472+ and netcoreapp2.2+.

Given a method that returns a named tuple:

<!-- snippet: MethodWithNamedTuple -->
```cs
static (bool Member1, string Member2, string Member3) MethodWithNamedTuple()
{
    return (true, "A", "B");
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L64-L69)</sup>
<!-- endsnippet -->

Can be verified:

<!-- snippet: VerifyTuple -->
```cs
ObjectApprover.VerifyTuple(() => MethodWithNamedTuple());
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.cs#L57-L61)</sup>
<!-- endsnippet -->

Resulting in:

<!-- snippet: ObjectApproverTests.NamedTuple.approved.txt -->
```txt
{
  Member1: true,
  Member2: 'A',
  Member3: 'B'
}
```
<sup>[snippet source](/src/Tests/ObjectApproverTests.NamedTuple.approved.txt#L1-L5)</sup>
<!-- endsnippet -->


## Icon

<a href="http://thenounproject.com/term/helmet/9554/">Helmet</a> designed by <a href="http://thenounproject.com/alterego">Leonidas Ikonomou</a> from The Noun Project
