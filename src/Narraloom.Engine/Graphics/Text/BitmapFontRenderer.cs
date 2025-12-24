using Silk.NET.OpenGL;
using Narraloom.Assets.Fonts;
using System;
using System.Collections.Generic;
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

	private float AdvanceOf(char ch, float scale)
	{
		if (ch == '\t') ch = ' ';
		if (_font.Glyphs.TryGetValue(ch, out var g))
			return g.XAdvance * scale;

		// fallback: a small advance
		return (_font.LineHeight * 0.33f) * scale;
	}

	private void AddGlyphQuads(List<float> floats, BitmapFont.Glyph g, float penX, float penY, float scale)
	{
		float gx = penX + g.XOffset * scale;
		float gy = penY + g.YOffset * scale;
		float gw = g.Width * scale;
		float gh = g.Height * scale;

		float u0 = g.X / (float)_font.ScaleW;
		float v0 = g.Y / (float)_font.ScaleH;
		float u1 = (g.X + g.Width) / (float)_font.ScaleW;
		float v1 = (g.Y + g.Height) / (float)_font.ScaleH;

		// tri1
		floats.Add(gx); floats.Add(gy); floats.Add(u0); floats.Add(v0);
		floats.Add(gx + gw); floats.Add(gy); floats.Add(u1); floats.Add(v0);
		floats.Add(gx + gw); floats.Add(gy + gh); floats.Add(u1); floats.Add(v1);
		// tri2
		floats.Add(gx); floats.Add(gy); floats.Add(u0); floats.Add(v0);
		floats.Add(gx + gw); floats.Add(gy + gh); floats.Add(u1); floats.Add(v1);
		floats.Add(gx); floats.Add(gy + gh); floats.Add(u0); floats.Add(v1);
	}

	private void Flush(List<float> floats, float r, float g, float b, float a)
	{
		if (floats.Count == 0) return;

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

	/// <summary>
	/// Draws text in a box with word-wrap.
	/// - maxWidth: wrap width in pixels
	/// - maxLines: if >0, clamps line count
	/// - visibleChars: if >=0, draws only that many characters (typewriter)
	/// Returns: (drawnChars, isFullyShown)
	/// </summary>
	public (int drawnChars, bool fullyShown) DrawTextWrapped(
		string text,
		float x,
		float y,
		float maxWidth,
		float scale,
		float r, float g, float b, float a,
		int maxLines = 0,
		int visibleChars = -1
	)
	{
		if (string.IsNullOrEmpty(text) || maxWidth <= 1)
			return (0, true);

		var floats = new List<float>(Math.Min(text.Length, 256) * 24);

		float penX = x;
		float penY = y;

		int line = 1;
		int drawn = 0;

		bool limitChars = visibleChars >= 0;

		// simple word-wrap tokenizer: build words including trailing space
		int i = 0;
		while (i < text.Length)
		{
			if (limitChars && drawn >= visibleChars)
				break;

			char c = text[i];

			// explicit newline
			if (c == '\n')
			{
				i++;
				drawn++;

				penX = x;
				penY += _font.LineHeight * scale;
				line++;

				if (maxLines > 0 && line > maxLines)
					return (drawn, false);

				continue;
			}

			// gather one "token": either a run of spaces OR a word (non-space run)
			int start = i;
			bool isSpace = c == ' ' || c == '\t';
			while (i < text.Length)
			{
				char cc = text[i];
				bool spaceNow = cc == ' ' || cc == '\t';
				if (cc == '\n') break;
				if (spaceNow != isSpace) break;
				i++;
			}
			string token = text.Substring(start, i - start);

			// measure token width
			float tokenWidth = 0f;
			for (int k = 0; k < token.Length; k++)
			{
				if (limitChars && (drawn + k) >= visibleChars)
					break;

				tokenWidth += AdvanceOf(token[k], scale);
			}

			// wrap rule: if token doesn't fit and it's not leading spaces, go next line
			bool wouldOverflow = (penX - x) + tokenWidth > maxWidth;
			if (wouldOverflow && !isSpace && (penX > x))
			{
				penX = x;
				penY += _font.LineHeight * scale;
				line++;

				if (maxLines > 0 && line > maxLines)
					return (drawn, false);
			}

			// draw token (respect visibleChars)
			for (int k = 0; k < token.Length; k++)
			{
				if (limitChars && drawn >= visibleChars)
					break;

				char ch = token[k];
				drawn++;

				if (ch == '\t') ch = ' ';

				if (ch == ' ')
				{
					penX += AdvanceOf(' ', scale);
					continue;
				}

				if (_font.Glyphs.TryGetValue(ch, out var glyph))
				{
					AddGlyphQuads(floats, glyph, penX, penY, scale);
					penX += glyph.XAdvance * scale;
				}
				else
				{
					penX += AdvanceOf(ch, scale);
				}
			}
		}

		Flush(floats, r, g, b, a);

		bool fully = !limitChars || drawn >= text.Length;
		if (limitChars) fully = (drawn >= Math.Min(text.Length, visibleChars)) && (visibleChars >= text.Length);

		// "fullyShown" here means: if visibleChars is used, did we reach end of string?
		bool fullyShown = !limitChars ? true : (visibleChars >= text.Length);
		return (drawn, fullyShown);
	}
}
