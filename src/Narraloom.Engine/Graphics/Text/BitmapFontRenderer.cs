using Silk.NET.OpenGL;
using Narraloom.Assets.Fonts;
using System;
using System.IO;

namespace Narraloom.Engine.Graphics.Text;

public sealed class BitmapFontRenderer
{
	private readonly GL _gl;
	private readonly BitmapFont _font;
	private readonly Texture2D _atlas;

	private uint _vao;
	private uint _vbo;
	private uint _shader;

	public BitmapFontRenderer(GL gl, BitmapFont font, Texture2D atlas)
	{
		_gl = gl;
		_font = font;
		_atlas = atlas;

		CreateShader();
		CreateBuffers();
	}

	private void CreateShader()
	{
		string vertexSource = File.ReadAllText("src/Narraloom.Engine/Graphics/Shaders/textured.vert");
		string fragmentSource = File.ReadAllText("src/Narraloom.Engine/Graphics/Shaders/textured.frag");

		uint vs = _gl.CreateShader(ShaderType.VertexShader);
		_gl.ShaderSource(vs, vertexSource);
		_gl.CompileShader(vs);

		uint fs = _gl.CreateShader(ShaderType.FragmentShader);
		_gl.ShaderSource(fs, fragmentSource);
		_gl.CompileShader(fs);

		_shader = _gl.CreateProgram();
		_gl.AttachShader(_shader, vs);
		_gl.AttachShader(_shader, fs);
		_gl.LinkProgram(_shader);

		_gl.DeleteShader(vs);
		_gl.DeleteShader(fs);
	}

	private void CreateBuffers()
	{
		// Interleaved: x,y,u,v
		_vao = _gl.GenVertexArray();
		_vbo = _gl.GenBuffer();

		_gl.BindVertexArray(_vao);
		_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

		uint stride = 4u * sizeof(float);

		_gl.EnableVertexAttribArray(0);
		_gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, IntPtr.Zero);

		_gl.EnableVertexAttribArray(1);
		_gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (IntPtr)(2 * sizeof(float)));

		_gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		_gl.BindVertexArray(0);
	}

	public void DrawText(string text, float x, float y, float scale, float r, float g, float b, float a = 1f)
	{
		if (string.IsNullOrEmpty(text))
			return;

		// Build a big vertex array for batching: 6 vertices per glyph, 4 floats per vertex
		// total floats = glyphCount * 6 * 4
		// We'll skip missing glyphs safely.
		var floats = new List<float>(text.Length * 24);

		float penX = x;
		float penY = y;

		foreach (char ch in text)
		{
			if (ch == '\n')
			{
				penX = x;
				penY += _font.LineHeight * scale;
				continue;
			}

			if (!_font.Glyphs.TryGetValue(ch, out var gph))
			{
				penX += (_font.LineHeight * 0.3f) * scale; // simple fallback advance
				continue;
			}

			// Position in pixels
			float gx = penX + gph.XOffset * scale;
			float gy = penY + gph.YOffset * scale;
			float gw = gph.Width * scale;
			float gh = gph.Height * scale;

			// UV (0..1)
			float u0 = gph.X / (float)_font.ScaleW;
			float v0 = gph.Y / (float)_font.ScaleH;
			float u1 = (gph.X + gph.Width) / (float)_font.ScaleW;
			float v1 = (gph.Y + gph.Height) / (float)_font.ScaleH;

			// 2 triangles (x,y,u,v)
			// tri1
			floats.Add(gx); floats.Add(gy); floats.Add(u0); floats.Add(v0);
			floats.Add(gx + gw); floats.Add(gy); floats.Add(u1); floats.Add(v0);
			floats.Add(gx + gw); floats.Add(gy + gh); floats.Add(u1); floats.Add(v1);
			// tri2
			floats.Add(gx); floats.Add(gy); floats.Add(u0); floats.Add(v0);
			floats.Add(gx + gw); floats.Add(gy + gh); floats.Add(u1); floats.Add(v1);
			floats.Add(gx); floats.Add(gy + gh); floats.Add(u0); floats.Add(v1);

			penX += gph.XAdvance * scale;
		}

		if (floats.Count == 0)
			return;

		_gl.UseProgram(_shader);

		_gl.Uniform2(_gl.GetUniformLocation(_shader, "uResolution"), 1280f, 720f);
		_gl.Uniform4(_gl.GetUniformLocation(_shader, "uColor"), r, g, b, a);

		_atlas.Bind(TextureUnit.Texture0);
		_gl.Uniform1(_gl.GetUniformLocation(_shader, "uTexture"), 0);

		_gl.BindVertexArray(_vao);
		_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

		_gl.BufferData<float>(BufferTargetARB.ArrayBuffer, floats.ToArray(), BufferUsageARB.DynamicDraw);

		int vertexCount = floats.Count / 4; // 4 floats per vertex
		_gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertexCount);

		_gl.BindVertexArray(0);
	}
}