namespace Clarion.Core.Profiles;

public enum CliMode { None, Help, Version, Apply, Clean, ListPresets, Verify, WhatBroke, ListSymptoms, History, ListSettings, ListCleanups, CatalogDoc }

public sealed record CliOptions(
    CliMode Mode,
    string? File = null,
    string? Preset = null,
    IReadOnlyList<string>? Only = null,
    IReadOnlyList<string>? Include = null,
    bool Preview = false,
    bool NoRestorePoint = false,
    bool EnableProtection = false,
    string? Symptom = null,
    DateTimeOffset? Since = null,
    bool SaveReport = false,
    string? Error = null)
{
    public bool IsCli => Mode != CliMode.None;

    /// <summary>Not for people. It marks a copy started by the window to get administrator rights.</summary>
    public const string RelaunchMarker = "--elevated-relaunch";

    /// <summary>Parses command line arguments. Unknown switches give an error instead of being ignored.</summary>
    public static CliOptions Parse(IReadOnlyList<string> args)
    {
        var mode = CliMode.None;
        string? file = null, preset = null;
        var only = new List<string>();
        var include = new List<string>();
        bool preview = false, noRp = false, enableProtection = false;
        string? symptom = null;
        DateTimeOffset? since = null;
        var saveReport = false;

        for (var i = 0; i < args.Count; i++)
        {
            var a = args[i];
            string? Next() => i + 1 < args.Count ? args[++i] : null;

            switch (a.ToLowerInvariant())
            {
                case "--help" or "-h" or "/?": mode = CliMode.Help; break;
                case "--version": mode = CliMode.Version; break;
                // Added by the window when it starts itself again with administrator rights, so that copy never tries a second time.
                case RelaunchMarker: break;
                case "--list-presets": mode = CliMode.ListPresets; break;
                case "--apply":
                    mode = CliMode.Apply;
                    file = Next();
                    if (file is null || file.StartsWith("--", StringComparison.Ordinal)) return Fail("--apply needs a setup file.");
                    break;
                case "--apply-preset":
                    mode = CliMode.Apply;
                    preset = Next();
                    if (preset is null || preset.StartsWith("--", StringComparison.Ordinal)) return Fail("--apply-preset needs a preset name.");
                    break;
                case "--clean": mode = CliMode.Clean; break;
                case "--only": only.AddRange(Split(Next())); break;
                case "--include": include.AddRange(Split(Next())); break;
                case "--preview": preview = true; break;
                case "--no-restore-point": noRp = true; break;
                case "--enable-protection": enableProtection = true; break;
                case "--verify": mode = CliMode.Verify; break;
                case "--save": saveReport = true; break;
                case "--history": mode = CliMode.History; break;
                case "--list-symptoms": mode = CliMode.ListSymptoms; break;
                case "--list-settings": mode = CliMode.ListSettings; break;
                case "--list-cleanups": mode = CliMode.ListCleanups; break;
                case "--catalog-doc": mode = CliMode.CatalogDoc; break;
                case "--what-broke":
                    mode = CliMode.WhatBroke;
                    symptom = Next();
                    if (symptom is null || symptom.StartsWith("--", StringComparison.Ordinal)) return Fail("--what-broke needs a symptom name. Use --list-symptoms.");
                    break;
                case "--since":
                    var when = Next();
                    if (!DateTimeOffset.TryParse(when, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var parsed))
                        return Fail("--since needs a date such as 2026-09-01.");
                    since = parsed;
                    break;
                default:
                    return a.StartsWith('-') || a.StartsWith('/') ? Fail($"Unknown option {a}. Use --help.") : new CliOptions(CliMode.None);
            }
        }
        return new CliOptions(mode, file, preset, only, include, preview, noRp, enableProtection, symptom, since, saveReport);
    }

    private static IEnumerable<string> Split(string? value) =>
        (value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static CliOptions Fail(string message) => new(CliMode.Help, Error: message);

    public const string HelpText = """
        Clarion command line

          Clarion.exe --apply setup.json [--preview] [--no-restore-point] [--enable-protection]
          Clarion.exe --apply-preset minimal|standard|advanced|gaming|privacy [--preview]
          Clarion.exe --clean [--only a,b] [--include c,d] [--preview]
          Clarion.exe --verify [--save]
          Clarion.exe --what-broke symptom [--since 2026-09-01]
          Clarion.exe --list-symptoms
          Clarion.exe --history
          Clarion.exe --list-presets
          Clarion.exe --list-settings
          Clarion.exe --list-cleanups
          Clarion.exe --catalog-doc
          Clarion.exe --version

        --preview           show what would change and change nothing
        --no-restore-point  skip the System Restore point
        --enable-protection turn on System Protection if it is off, so the restore point can be made
        --only / --include  cleanup row names, such as shaders.nvidia,windows.temp. --list-cleanups shows them all
        --list-settings     list every setting with the name a setup file uses for it
        --catalog-doc       print the settings reference as Markdown, for docs/CATALOG.md

        --verify            read only: check what Clarion applied against Windows now. Exit code 1 when something changed back
        --save              with --verify, keep the result so Clarion can show it next time it is opened. The exit code is then 0, since finding changes is not a failure (the monthly check uses this)
        --what-broke        read only: list the changes that could explain a symptom, newest first
        --since             with --what-broke, only changes made on or after this date
        --history           read only: list what Clarion changed that is still on

        The read only commands need no administrator rights, though some checks cannot be read without them.
        Exit codes: 0 done, 1 some items did not finish or something changed back, 2 bad input, 3 needs administrator rights.
        """;
}
