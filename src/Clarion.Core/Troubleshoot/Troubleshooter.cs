using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Core.Troubleshoot;

/// <summary>Something that can stop working, in the words a person would use, and where to look.</summary>
/// <param name="Topics">Every setting in these catalog topics is a suspect.</param>
/// <param name="TweakIds">Specific settings that are suspects even when their topic is not listed.</param>
public sealed record Symptom(string Id, string Title, string Tip, IReadOnlyList<string> Topics, IReadOnlyList<string> TweakIds);

/// <summary>A setting Clarion changed that could explain a symptom.</summary>
/// <param name="Direct">True when the setting is named for this symptom, false when it only shares a topic.</param>
public sealed record Suspect(Tweak Tweak, DateTimeOffset AppliedAt, bool Direct);

/// <summary>
/// Turns the journal into a diagnostic: given a symptom, lists the settings Clarion changed that
/// could cause it. Only settings still on are listed, named ones first, then the newest change first.
/// </summary>
public sealed class Troubleshooter(ChangeJournal journal)
{
    public IReadOnlyList<Suspect> Suspects(Symptom symptom, IEnumerable<Tweak> catalog)
    {
        var outstanding = journal.OutstandingByTweak();
        var named = symptom.TweakIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var topics = symptom.Topics.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return catalog
            .Where(t => outstanding.ContainsKey(t.Id))
            .Select(t => (Tweak: t, Direct: named.Contains(t.Id), ByTopic: topics.Contains(t.Topic)))
            .Where(x => x.Direct || x.ByTopic)
            .Select(x => new Suspect(x.Tweak, outstanding[x.Tweak.Id][0].Time, x.Direct))
            .OrderByDescending(s => s.Direct)
            .ThenByDescending(s => s.AppliedAt)
            .ToList();
    }

    /// <summary>Checks the symptom list against the catalog. Returns problems, empty when it is sound.</summary>
    public static IReadOnlyList<string> Validate(IEnumerable<Symptom> symptoms, IReadOnlyList<Tweak> catalog)
    {
        var errors = new List<string>();
        var ids = catalog.Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var topics = catalog.Select(t => t.Topic).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in symptoms)
        {
            if (string.IsNullOrWhiteSpace(s.Id) || !seen.Add(s.Id)) errors.Add($"{s.Id}: missing or duplicate id.");
            if (string.IsNullOrWhiteSpace(s.Title)) errors.Add($"{s.Id}: title is empty.");
            if (string.IsNullOrWhiteSpace(s.Tip)) errors.Add($"{s.Id}: tip is empty.");
            if (s.Topics.Count == 0 && s.TweakIds.Count == 0) errors.Add($"{s.Id}: names no topics or settings.");
            foreach (var topic in s.Topics.Where(t => !topics.Contains(t))) errors.Add($"{s.Id}: unknown topic {topic}.");
            foreach (var id in s.TweakIds.Where(i => !ids.Contains(i))) errors.Add($"{s.Id}: unknown setting {id}.");
        }
        return errors;
    }
}
