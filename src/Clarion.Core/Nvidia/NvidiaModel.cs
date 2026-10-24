namespace Clarion.Core.Nvidia;

/// <summary>
/// Reads and writes the NVIDIA graphics driver's global profile through NVIDIA's own NVAPI. A value that is not stored in the profile
/// reads as null, and the driver's default applies to it.
/// </summary>
public interface INvidiaDriverStore
{
    /// <summary>True when NVAPI starts and an NVIDIA graphics card with a driver is in this PC.</summary>
    bool IsAvailable();

    /// <summary>The stored value, or null when the profile holds none and the driver default applies.</summary>
    uint? Read(uint id);

    /// <summary>
    /// The stored values of several settings. A store that has to open the driver's settings for every read can override this to open them
    /// once, which is what makes reading a handful of settings quick.
    /// </summary>
    IReadOnlyDictionary<uint, uint?> ReadMany(IEnumerable<uint> ids) => ids.Distinct().ToDictionary(id => id, Read);

    void Write(uint id, uint value);

    /// <summary>Removes the stored value, so the driver default applies again.</summary>
    void Reset(uint id);
}

/// <summary>
/// One driver setting that .Clarion is allowed to change. The numbers are the setting ids and values NVIDIA publishes in
/// NvApiDriverSettings.h of its public NVAPI repository (github.com/NVIDIA/nvapi, MIT licence). Anything not listed here is refused
/// when the catalog loads, so a typing mistake cannot write to a setting nobody checked.
/// </summary>
public sealed class NvidiaSetting
{
    private readonly Func<uint, bool> _isValid;
    private readonly Func<uint, string> _describe;

    public NvidiaSetting(uint id, string name, Func<uint, bool> isValid, Func<uint, string> describe)
    {
        Id = id;
        Name = name;
        _isValid = isValid;
        _describe = describe;
    }

    public uint Id { get; }

    /// <summary>The name the driver itself gives the setting.</summary>
    public string Name { get; }

    public bool IsValid(uint value) => _isValid(value);

    public string Describe(uint value) => _describe(value);
}

public static class NvidiaSettings
{
    public const uint MaxPreRenderedFrames = 0x007BA09E;
    public const uint PowerManagementMode = 0x1057EB71;
    public const uint ShaderCacheMaxSize = 0x00AC8497;
    public const uint TextureFilteringQuality = 0x00CE2691;

    public const uint Unlimited = 0xFFFFFFFF;

    public static IReadOnlyList<NvidiaSetting> All { get; } =
    [
        // PRERENDERLIMIT: 0 leaves it to the application (the default), 1 to 255 is the most frames the CPU may queue ahead.
        new(MaxPreRenderedFrames, "Maximum pre-rendered frames", v => v <= 255,
            v => v == 0 ? "left to the application" : v == 1 ? "1 frame" : $"{v} frames"),

        // PREFERRED_PSTATE: 0 adaptive, 1 prefer maximum performance, 2 driver controlled, 3 prefer consistent performance,
        // 4 prefer minimum power, 5 optimal power.
        new(PowerManagementMode, "Power management mode", v => v <= 5, v => v switch
        {
            0 => "adaptive",
            1 => "prefer maximum performance",
            2 => "driver controlled",
            3 => "prefer consistent performance",
            4 => "prefer minimum power",
            5 => "optimal power",
            _ => v.ToString(),
        }),

        // PS_SHADERDISKCACHE_MAX_SIZE: a size in megabytes. 0xFFFFFFFF is no limit, and the driver's default is 0x4000 (16 GB).
        new(ShaderCacheMaxSize, "Shader disk cache maximum size", _ => true,
            v => v == Unlimited ? "no limit" : $"{v} MB"),

        // QUALITY_ENHANCEMENTS, shown as Texture filtering - Quality.
        new(TextureFilteringQuality, "Texture filtering - Quality", v => v is 0xFFFFFFF6 or 0 or 0x0A or 0x14, v => v switch
        {
            0xFFFFFFF6 => "high quality",
            0 => "quality",
            0x0A => "performance",
            0x14 => "high performance",
            _ => v.ToString(),
        }),
    ];

    public static NvidiaSetting? Find(uint id) => All.FirstOrDefault(s => s.Id == id);
}
