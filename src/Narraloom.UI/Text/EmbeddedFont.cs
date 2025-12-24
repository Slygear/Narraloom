namespace Narraloom.UI.Text;

/// <summary>
/// Temporary embedded bitmap font (ASCII 32–126)
/// Each glyph is 8x8 pixels.
/// This is a PHASE 1 font. Will be replaced later.
/// </summary>
public static class EmbeddedFont
{
    public const int GlyphWidth = 8;
    public const int GlyphHeight = 8;

    // Each byte = one row (1 = pixel on)
    // Example font based on classic terminal font
    public static readonly Dictionary<char, byte[]> Glyphs = new()
    {
        ['A'] = new byte[]
        {
            0b_00111100,
            0b_01000010,
            0b_01000010,
            0b_01111110,
            0b_01000010,
            0b_01000010,
            0b_01000010,
            0b_00000000
        },
        ['B'] = new byte[]
        {
            0b_01111100,
            0b_01000010,
            0b_01000010,
            0b_01111100,
            0b_01000010,
            0b_01000010,
            0b_01111100,
            0b_00000000
        },
        [' '] = new byte[]
        {
            0,0,0,0,0,0,0,0
        },
        ['.'] = new byte[]
        {
            0,0,0,0,0,0,
            0b_00110000,
            0b_00110000
        }
    };
}
