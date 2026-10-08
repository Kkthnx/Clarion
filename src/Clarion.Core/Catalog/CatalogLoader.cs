using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clarion.Core.Model;

namespace Clarion.Core.Catalog;

public static class CatalogLoader
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static IReadOnlyList<Tweak> LoadEmbedded()
    {
        var asm = typeof(CatalogLoader).Assembly;
        var all = new List<Tweak>();
        foreach (var name in asm.GetManifestResourceNames().Where(n => n.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).Order())
        {
            using var stream = asm.GetManifestResourceStream(name)!;
            var items = JsonSerializer.Deserialize<List<Tweak>>(stream, Options)
                        ?? throw new InvalidDataException($"Empty catalog file {name}");
            all.AddRange(items);
        }
        return all;
    }

    public static IReadOnlyList<Tweak> Parse(string json) =>
        JsonSerializer.Deserialize<List<Tweak>>(json, Options) ?? [];

    /// <summary>Returns a list of problems. An empty list means the catalog is valid.</summary>
    public static IReadOnlyList<string> Validate(IEnumerable<Tweak> tweaks)
    {
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in tweaks)
        {
            if (string.IsNullOrWhiteSpace(t.Id)) { errors.Add("A tweak has no id."); continue; }
            if (!seen.Add(t.Id)) errors.Add($"{t.Id}: duplicate id.");

            foreach (var (label, text) in new[] { ("name", t.Name), ("summary", t.Summary), ("what", t.What), ("benefit", t.Benefit), ("risk", t.Risk) })
            {
                if (string.IsNullOrWhiteSpace(text)) errors.Add($"{t.Id}: {label} is empty.");
            }

            if (t.Apply.Count == 0) errors.Add($"{t.Id}: no apply operations.");
            if (t.Evidence is Evidence.Proven && t.Sources.Count == 0) errors.Add($"{t.Id}: Proven evidence needs at least one source.");

            foreach (var op in t.Apply.Concat(t.Undo))
            {
                var target = op switch
                {
                    SetRegistryValue s => s.Target,
                    DeleteRegistryValue d => d.Target,
                    _ => null,
                };
                if (target is null) { errors.Add($"{t.Id}: unknown operation type."); continue; }
                if (string.IsNullOrWhiteSpace(target.Path) || target.Path.StartsWith('\\') || target.Path.StartsWith("HK", StringComparison.OrdinalIgnoreCase))
                    errors.Add($"{t.Id}: registry path must be relative to the hive: {target.Path}");
                if (target.Hive == RegistryHive.LocalMachine && t.Scope != TweakScope.Machine)
                    errors.Add($"{t.Id}: touches HKLM but scope is not Machine.");
                if (op is SetRegistryValue set)
                {
                    if (set.Data.Kind is RegistryKind.DWord && !uint.TryParse(set.Data.Value, out _))
                        errors.Add($"{t.Id}: DWord value is not a valid unsigned number: {set.Data.Value}");
                    if (set.Data.Kind is RegistryKind.QWord && !ulong.TryParse(set.Data.Value, out _))
                        errors.Add($"{t.Id}: QWord value is not a valid unsigned number: {set.Data.Value}");
                }
            }
        }
        return errors;
    }
}
