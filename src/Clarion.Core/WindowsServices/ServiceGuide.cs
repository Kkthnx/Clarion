using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Clarion.Core.Model;

namespace Clarion.Core.WindowsServices;

/// <summary>The five recommendations in Microsoft's table of Windows services.</summary>
public enum ServiceAdvice { OkToDisable, DontDisable, NoGuidance, AlreadyDisabled, ShouldDisable }

/// <summary>What Microsoft's table says about one service: its name, how Windows starts it by default, and the recommendation.</summary>
public sealed record GuideEntry(string Name, string Display, ServiceStartType Default, ServiceAdvice Advice, bool PerUser);

/// <summary>
/// Microsoft's "Guidance on configuring system services" for Windows IoT Enterprise, kept as data: 255 services with their default
/// start type and one of five recommendations. It is written for fixed function devices, so it says what is safe to switch off, not
/// what will speed a PC up. Only facts are kept (names, default start types, the recommendation), not Microsoft's descriptions.
/// </summary>
public sealed partial class ServiceGuide
{
    private sealed record RawEntry(string Name, string Display, ServiceStartType Default, ServiceAdvice Recommendation, bool PerUser = false);

    private sealed record RawGuide(string Source, string Title, string Dated, List<RawEntry> Services);

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>A per-user service runs as one copy for each signed in user, named like CDPUserSvc_4a2b3, after the name in the table.</summary>
    [GeneratedRegex("^(?<template>.+)_[0-9a-fA-F]{4,8}$")]
    private static partial Regex InstanceName();

    private readonly Dictionary<string, GuideEntry> _byName;

    private ServiceGuide(string source, string title, string dated, IReadOnlyList<GuideEntry> entries)
    {
        Source = source;
        Title = title;
        Dated = dated;
        Entries = entries;
        _byName = entries.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);
    }

    public string Source { get; }

    public string Title { get; }

    /// <summary>The date on Microsoft's page when the table was read.</summary>
    public string Dated { get; }

    public IReadOnlyList<GuideEntry> Entries { get; }

    public static ServiceGuide LoadEmbedded()
    {
        var asm = typeof(ServiceGuide).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith(".Catalog.Services.services-guide.json", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    public static ServiceGuide Parse(string json)
    {
        var raw = JsonSerializer.Deserialize<RawGuide>(json, Options) ?? throw new JsonException("The services guide is empty.");
        var entries = raw.Services.Select(s => new GuideEntry(s.Name, s.Display, s.Default, s.Recommendation, s.PerUser)).ToList();
        return new ServiceGuide(raw.Source, raw.Title, raw.Dated, entries);
    }

    /// <summary>The entry for a service name, or for the template of a per-user service instance. Null when Microsoft's table does not list it.</summary>
    public GuideEntry? Find(string serviceName)
    {
        if (_byName.TryGetValue(serviceName, out var exact)) return exact;
        var m = InstanceName().Match(serviceName);
        return m.Success && _byName.TryGetValue(m.Groups["template"].Value, out var perUser) && perUser.PerUser ? perUser : null;
    }

    /// <summary>The name a per-user instance is shown under, which is the template's. Other names are returned as they are.</summary>
    public string TemplateName(string serviceName) => Find(serviceName)?.Name ?? serviceName;

    public static string Label(ServiceAdvice a) => a switch
    {
        ServiceAdvice.OkToDisable => "OK to disable",
        ServiceAdvice.DontDisable => "Don't disable",
        ServiceAdvice.NoGuidance => "No guidance",
        ServiceAdvice.AlreadyDisabled => "Off by default",
        _ => "Should be disabled",
    };

    /// <summary>What a recommendation means, in the words of Microsoft's own definitions.</summary>
    public static string Hint(ServiceAdvice a) => a switch
    {
        ServiceAdvice.OkToDisable => "Useful to some people but not all. Microsoft says anyone who does not use it can switch it off. That is not a promise that it speeds anything up.",
        ServiceAdvice.DontDisable => "Switching it off stops features Windows needs. Microsoft says to leave it as it is.",
        ServiceAdvice.NoGuidance => "Microsoft has not worked out what switching it off does, so it says to leave the default.",
        ServiceAdvice.AlreadyDisabled => "Windows ships it switched off, so there is nothing to change.",
        _ => "Microsoft says it should never be on for a well managed enterprise PC. Check what you use before switching it off.",
    };

    public static string StartLabel(ServiceStartType t) => t switch
    {
        ServiceStartType.Automatic => "Automatic",
        ServiceStartType.AutomaticDelayed => "Automatic, delayed",
        ServiceStartType.Manual => "Manual",
        _ => "Disabled",
    };
}
