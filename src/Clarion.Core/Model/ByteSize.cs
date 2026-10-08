namespace Clarion.Core.Model;

/// <summary>Turns a byte count into text such as 2.0 GB.</summary>
public static class ByteSize
{
    public static string Format(long bytes) => bytes switch
    {
        >= 1L << 30 => $"{bytes / (double)(1L << 30):0.0} GB",
        >= 1L << 20 => $"{bytes / (double)(1L << 20):0.0} MB",
        >= 1L << 10 => $"{bytes / (double)(1L << 10):0.0} KB",
        _ => $"{bytes} bytes",
    };
}
