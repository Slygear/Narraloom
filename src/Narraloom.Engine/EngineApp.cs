using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Narraloom.Engine.Graphics;
using Narraloom.Engine.Graphics.Text;
using Narraloom.Assets.Fonts;

namespace Narraloom.Engine;

public class EngineApp
{
    private IWindow _window = null!;
    private GL _gl = null!;
    private IInputContext _input = null!;
    private IKeyboard _keyboard = null!;

    private SolidQuadRenderer _panel = null!;
    private BitmapFontRenderer _fontRenderer = null!;

    private BitmapFont _font = null!;
    private Texture2D _fontAtlas = null!;

    private string _demoText = "Narraloom is alive.";
    private int _demoChars = 0;
    private double _timer = 0;

    public void Run()
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
        options.Title = "Narraloom Player";

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;

        _window.Run();
    }

    private void OnLoad()
    {
        _gl = GL.GetApi(_window);

        _gl.ClearColor(0f, 0f, 0f, 1f);

        // Alpha blending (text needs this)
        _gl.Enable(GLEnum.Blend);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        // Input
        _input = _window.CreateInput();
        _keyboard = _input.Keyboards[0];
        _keyboard.KeyDown += (_, key, _) =>
        {
            if (key == Key.Escape) _window.Close();
            if (key == Key.Space) _demoChars = _demoText.Length; // skip typewriter
        };

        // Renderers
        _panel = new SolidQuadRenderer(_gl);

        // Load BMFont
        _font = BitmapFontLoader.Load("assets/fonts/default.fnt");
        _fontAtlas = new Texture2D(_gl, "assets/fonts/default.png");

        _fontRenderer = new BitmapFontRenderer(_gl, _font, _fontAtlas);
    }

    private void OnUpdate(double delta)
    {
        // demo typewriter
        _timer += delta;
        if (_demoChars < _demoText.Length && _timer >= 0.03)
        {
            _demoChars++;
            _timer = 0;
        }
    }

    private void OnRender(double delta)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Dialogue panel
        _panel.DrawRect(
            50, 480, 1180, 180,
            0.15f, 0.15f, 0.15f, 1f
        );

        // Text (top-left inside panel)
        string visible = _demoText.Substring(0, Math.Min(_demoChars, _demoText.Length));
        _fontRenderer.DrawText(
            visible,
            80, 520,   // x, y
            1.0f,      // scale
            1f, 1f, 1f, 1f
        );
    }
}