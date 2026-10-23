using System.Globalization;
using System.Text;

namespace Clarion.Core.Apps;

/// <summary>
/// Rewrites SVG path data with every coordinate absolute. SVG says that a relative move after a close starts from where the closed shape
/// began, and the drawing code of WinUI does not agree for every shape, so a logo made of several pieces (a word, a mark with a hole)
/// came out with its pieces in the wrong places. Absolute coordinates mean the same thing everywhere.
/// </summary>
public static class SvgPath
{
    public static string ToAbsolute(string data)
    {
        var s = new Scanner(data);
        var sb = new StringBuilder(data.Length + 16);
        double cx = 0, cy = 0, startX = 0, startY = 0;
        var command = ' ';

        void Point(char letter, params double[] values)
        {
            sb.Append(letter);
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(Number(values[i]));
            }
        }

        while (s.SkipSeparators())
        {
            if (char.IsLetter(s.Peek()))
            {
                command = s.Next();
                if (command is 'Z' or 'z')
                {
                    sb.Append('Z');
                    cx = startX;
                    cy = startY;
                    continue;
                }
            }
            else if (command == ' ') throw new FormatException("The path does not start with a command.");

            var relative = char.IsLower(command);
            var (ox, oy) = relative ? (cx, cy) : (0.0, 0.0);
            switch (char.ToUpperInvariant(command))
            {
                case 'M':
                    (cx, cy) = (s.Number() + ox, s.Number() + oy);
                    (startX, startY) = (cx, cy);
                    Point('M', cx, cy);
                    // Pairs after the first are lines, as in SVG.
                    command = relative ? 'l' : 'L';
                    break;
                case 'L':
                    (cx, cy) = (s.Number() + ox, s.Number() + oy);
                    Point('L', cx, cy);
                    break;
                case 'H':
                    cx = s.Number() + ox;
                    Point('H', cx);
                    break;
                case 'V':
                    cy = s.Number() + oy;
                    Point('V', cy);
                    break;
                case 'C':
                    {
                        var (x1, y1, x2, y2, x, y) = (s.Number() + ox, s.Number() + oy, s.Number() + ox, s.Number() + oy, s.Number() + ox, s.Number() + oy);
                        Point('C', x1, y1, x2, y2, x, y);
                        (cx, cy) = (x, y);
                        break;
                    }
                case 'S':
                    {
                        var (x2, y2, x, y) = (s.Number() + ox, s.Number() + oy, s.Number() + ox, s.Number() + oy);
                        Point('S', x2, y2, x, y);
                        (cx, cy) = (x, y);
                        break;
                    }
                case 'Q':
                    {
                        var (x1, y1, x, y) = (s.Number() + ox, s.Number() + oy, s.Number() + ox, s.Number() + oy);
                        Point('Q', x1, y1, x, y);
                        (cx, cy) = (x, y);
                        break;
                    }
                case 'T':
                    (cx, cy) = (s.Number() + ox, s.Number() + oy);
                    Point('T', cx, cy);
                    break;
                case 'A':
                    {
                        var (rx, ry, rotation) = (s.Number(), s.Number(), s.Number());
                        var (large, sweep) = (s.Flag(), s.Flag());
                        (cx, cy) = (s.Number() + ox, s.Number() + oy);
                        Point('A', rx, ry, rotation, large, sweep, cx, cy);
                        break;
                    }
                default:
                    throw new FormatException($"Unknown path command {command}.");
            }
        }
        return sb.ToString();
    }

    private static string Number(double value)
    {
        var text = Math.Round(value, 4).ToString("0.####", CultureInfo.InvariantCulture);
        return text == "-0" ? "0" : text;
    }
    /// <summary>Reads numbers the way SVG lets them be written: run together ("1.5.5", "-2-3") and with the two flags of an arc side by side.</summary>
    private sealed class Scanner(string text)
    {
        private int _at;

        public bool SkipSeparators()
        {
            while (_at < text.Length && (char.IsWhiteSpace(text[_at]) || text[_at] == ',')) _at++;
            return _at < text.Length;
        }

        public char Peek() => text[_at];

        public char Next() => text[_at++];

        public double Flag()
        {
            SkipSeparators();
            if (_at >= text.Length || text[_at] is not ('0' or '1')) throw new FormatException("An arc flag must be 0 or 1.");
            return text[_at++] - '0';
        }

        public double Number()
        {
            SkipSeparators();
            var start = _at;
            if (_at < text.Length && text[_at] is '-' or '+') _at++;
            while (_at < text.Length && char.IsDigit(text[_at])) _at++;
            if (_at < text.Length && text[_at] == '.')
            {
                _at++;
                while (_at < text.Length && char.IsDigit(text[_at])) _at++;
            }
            if (_at < text.Length && text[_at] is 'e' or 'E')
            {
                _at++;
                if (_at < text.Length && text[_at] is '-' or '+') _at++;
                while (_at < text.Length && char.IsDigit(text[_at])) _at++;
            }
            if (start == _at) throw new FormatException($"A number was expected at position {start}.");
            return double.Parse(text.AsSpan(start, _at - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}
