using Silk.NET.OpenGL;
using System;
using System.IO;

namespace Narraloom.Engine.Graphics;

public sealed class SolidQuadRenderer
{
    private readonly GL _gl;
    private uint _vao;
    private uint _vbo;
    private uint _shader;

    public SolidQuadRenderer(GL gl)
    {
        _gl = gl;
        CreateShader();
        CreateBuffers();
    }

    private void CreateShader()
    {
        string vertexSource = File.ReadAllText("src/Narraloom.Engine/Graphics/Shaders/simple.vert");
        string fragmentSource = File.ReadAllText("src/Narraloom.Engine/Graphics/Shaders/simple.frag");

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
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), IntPtr.Zero);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }

    public void DrawRect(float x, float y, float w, float h, float r, float g, float b, float a = 1f)
    {
        float[] verts =
        {
            x,     y,
            x + w, y,
            x + w, y + h,

            x,     y,
            x + w, y + h,
            x,     y + h
        };

        _gl.UseProgram(_shader);
        _gl.Uniform2(_gl.GetUniformLocation(_shader, "uResolution"), 1280f, 720f);
        _gl.Uniform4(_gl.GetUniformLocation(_shader, "uColor"), r, g, b, a);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData<float>(BufferTargetARB.ArrayBuffer, verts, BufferUsageARB.DynamicDraw);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);
    }
}