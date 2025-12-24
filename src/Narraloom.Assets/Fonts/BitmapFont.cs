namespace Narraloom.Assets.Fonts;

public sealed class BitmapFont
{
    public sealed record Glyph(
        int Id,
        int X,
        int Y,
        int Width,
        int Height,
        int XOffset,
        int YOffset,
        int XAdvance
    );

    public int LineHeight { get; set; }
    public int ScaleW { get; set; }  // texture width
    public int ScaleH { get; set; }  // texture height

    public Dictionary<int, Glyph> Glyphs { get; } = new();
}
