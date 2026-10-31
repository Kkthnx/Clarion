namespace Clarion.Core.SystemInfo;

/// <summary>
/// Reads the two facts about the machine that the firmware table holds, the chassis type and the maker. The table is the one Windows hands out
/// as RSMB: a four byte header, a length, then structures that each start with a type, a length and a handle, followed by their strings.
/// Written against the DMTF SMBIOS reference, where the System Information structure is type 1 and the Chassis structure is type 3.
/// </summary>
public static class SmbiosTable
{
    private const int HeaderSize = 8;
    private const int SystemInformation = 1, Chassis = 3, EndOfTable = 127;

    /// <summary>The chassis types in the table. The top bit of the byte only says the chassis has a lock, so it is dropped.</summary>
    public static IReadOnlyList<int> ChassisTypes(byte[] raw)
    {
        var found = new List<int>();
        foreach (var s in Structures(raw))
            if (s.Type == Chassis && s.Length > 5) found.Add(raw[s.Start + 5] & 0x7F);
        return found;
    }

    /// <summary>The maker from the System Information structure, or an empty string.</summary>
    public static string Maker(byte[] raw)
    {
        foreach (var s in Structures(raw))
            if (s.Type == SystemInformation && s.Length > 4) return Text(raw, s, raw[s.Start + 4]);
        return "";
    }

    private readonly record struct Entry(int Type, int Start, int Length, int StringsStart, int End);

    private static IEnumerable<Entry> Structures(byte[] raw)
    {
        if (raw.Length <= HeaderSize) yield break;
        var declared = BitConverter.ToInt32(raw, 4);
        var limit = declared > 0 && HeaderSize + declared <= raw.Length ? HeaderSize + declared : raw.Length;
        var at = HeaderSize;
        while (at + 4 <= limit)
        {
            var type = raw[at];
            var length = raw[at + 1];
            if (length < 4) yield break;
            var stringsStart = at + length;
            // The strings end with two zero bytes. A table that stops short is not read any further.
            var end = stringsStart;
            while (end + 1 < limit && !(raw[end] == 0 && raw[end + 1] == 0)) end++;
            if (end + 1 >= limit) yield break;
            end += 2;
            yield return new Entry(type, at, length, stringsStart, end);
            if (type == EndOfTable) yield break;
            at = end;
        }
    }

    private static string Text(byte[] raw, Entry e, int index)
    {
        if (index == 0) return "";
        var at = e.StringsStart;
        for (var i = 1; at < e.End; i++)
        {
            var stop = Array.IndexOf(raw, (byte)0, at, e.End - at);
            if (stop < 0) return "";
            if (i == index) return System.Text.Encoding.ASCII.GetString(raw, at, stop - at).Trim();
            at = stop + 1;
        }
        return "";
    }
}
