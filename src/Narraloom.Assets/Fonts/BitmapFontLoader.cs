using System.Globalization;

namespace Narraloom.Assets.Fonts;

public static class BitmapFontLoader
{
    public static BitmapFont Load(string fntPath)
    {
        if (!File.Exists(fntPath))
            throw new FileNotFoundException("Font .fnt not found", fntPath);

        var font = new BitmapFont();
        var lines = File.ReadAllLines(fntPath);

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith("common "))
            {
                var kv = ParseKeyValues(line);
                font.LineHeight = GetInt(kv, "lineHeight");
                font.ScaleW = GetInt(kv, "scaleW");
                font.ScaleH = GetInt(kv, "scaleH");
            }
            else if (line.StartsWith("char "))
            {
                var kv = ParseKeyValues(line);

                int id = GetInt(kv, "id");
                var glyph = new BitmapFont.Glyph(
                    id,
                    GetInt(kv, "x"),
                    GetInt(kv, "y"),
                    GetInt(kv, "width"),
                    GetInt(kv, "height"),
                    GetInt(kv, "xoffset"),
                    GetInt(kv, "yoffset"),
                    GetInt(kv, "xadvance")
                );

                font.Glyphs[id] = glyph;
            }
        }

        // Basic sanity
        if (font.ScaleW <= 0 || font.ScaleH <= 0)
            throw new InvalidDataException("BMFont common scaleW/scaleH missing. Export as BMFont TEXT .fnt.");
        if (font.LineHeight <= 0)
            throw new InvalidDataException("BMFont common lineHeight missing. Export as BMFont TEXT .fnt.");

        return font;
    }

    private static Dictionary<string, string> ParseKeyValues(string line)
    {
        // "char id=65 x=.. y=.."
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // skip first token (e.g. "char" / "common")
        for (int i = 1; i < parts.Length; i++)
        {
            var p = parts[i];
            int eq = p.IndexOf('=');
            if (eq <= 0) continue;

            var key = p[..eq];
            var val = p[(eq + 1)..].Trim().Trim('"');
            dict[key] = val;
        }

        return dict;
    }

    private static int GetInt(Dictionary<string, string> kv, string key)
    {
        if (!kv.TryGetValue(key, out var s))
            return 0;

        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
            return v;

        return 0;
    }
}