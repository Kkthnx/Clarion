namespace Clarion.Core.Profiles;

public enum CliMode { None, Help, Version, Apply, Clean, ListPresets }

public sealed record CliOptions(
    CliMode Mode,
    string? File = null,
    string? Preset = null,
    IReadOnlyList<string>? Only = null,
    IReadOnlyList<string>? Include = null,
    bool Preview = false,
    bool NoRestorePoint = false,
    bool EnableProtection = false,
    string? Error = null)
{
    public bool IsCli => Mode != CliMode.None;

    /// <summary>Parses command line arguments. Unknown switches give an error instead of being ignored.</summary>
    public static CliOptions Parse(IReadOnlyList<string> args)
    {
        var mode = CliMode.None;
        string? file = null, preset = null;
        var only = new List<string>();
        var include = new List<string>();
        bool preview = false, noRp = false, enableProtection = false;

        for (var i = 0; i < args.Count; i++)
        {
            var a = args[i];
            string? Next() => i + 1 < args.Count ? args[++i] : null;

            switch (a.ToLowerInvariant())
            {
                case "--help" or "-h" or "/?": mode = CliMode.Help; break;
                case "--version": mode = CliMode.Version; break;
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
                default:
                    return a.StartsWith('-') || a.StartsWith('/') ? Fail($"Unknown option {a}. Use --help.") : new CliOptions(CliMode.None);
            }
        }
        return new CliOptions(mode, file, preset, only, include, preview, noRp, enableProtection);
    }

    private static IEnumerable<string> Split(string? value) =>
        (value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static CliOptions Fail(string message) => new(CliMode.Help, Error: message);

    public const string HelpText = """
        Clarion command line

          Clarion.exe --apply setup.json [--preview] [--no-restore-point] [--enable-protection]
          Clarion.exe --apply-preset minimal|standard|advanced|gaming|privacy [--preview]
          Clarion.exe --clean [--only a,b] [--include c,d] [--preview]
          Clarion.exe --list-presets
          Clarion.exe --version

        --preview           show what would change and change nothing
        --no-restore-point  skip the System Restore point
        --enable-protection turn on System Protection if it is off, so the restore point can be made
        --only / --include  cleanup row names, such as shaders.nvidia,windows.temp

        Exit codes: 0 done, 1 some items did not finish, 2 bad input, 3 needs administrator rights.
        """;
}
