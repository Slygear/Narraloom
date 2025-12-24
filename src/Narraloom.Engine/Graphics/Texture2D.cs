using Silk.NET.OpenGL;
using StbImageSharp;

namespace Narraloom.Engine.Graphics;

public sealed class Texture2D
{
    private readonly GL _gl;

    public uint Handle { get; }
    public int Width { get; }
    public int Height { get; }

    public Texture2D(GL gl, string path)
    {
        _gl = gl;

        if (!File.Exists(path))
            throw new FileNotFoundException("Texture file not found", path);

        using var stream = File.OpenRead(path);
        var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        Width = img.Width;
        Height = img.Height;

        Handle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, Handle);

        // Upload without unsafe
        _gl.TexImage2D<byte>(
    TextureTarget.Texture2D,
    0,
    InternalFormat.Rgba,
    (uint)Width,
    (uint)Height,
    0,
    PixelFormat.Rgba,
    PixelType.UnsignedByte,
    in img.Data[0]
);

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, Handle);
    }
}
