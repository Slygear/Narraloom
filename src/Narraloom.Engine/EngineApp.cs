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

    // ---- VN text config (Phase 1.1) ----
    private readonly float _panelX = 50;
    private readonly float _panelY = 480;
    private readonly float _panelW = 1180;
    private readonly float _panelH = 180;

    private readonly float _padL = 30;
    private readonly float _padT = 35;
    private readonly float _padR = 30;
    private readonly float _padB = 25;

    private readonly float _textScale = 1.0f;

    // Typewriter speed: chars per second (CPS)
    private double _cps = 40; // change this to taste
    private double _charAccumulator = 0;

    private string _text =
        "Narraloom is alive. This is Phase 1.1: word wrap, padding, newline.\n" +
        "Now we can write longer sentences and the engine will wrap them nicely inside the dialogue box.";

    private int _visibleChars = 0;
    private bool _completed = false;

    // continue indicator blink
    private double _blinkTimer = 0;
    private bool _blinkOn = true;

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

            // Space = typewriter skip OR next (placeholder)
            if (key == Key.Space)
            {
                if (!_completed)
                {
                    _visibleChars = _text.Length; // skip to end
                    _completed = true;
                }
                else
                {
                    // Phase 2: "Next line" / "advance"
                    // For now, just restart demo
                    _visibleChars = 0;
                    _completed = false;
                    _charAccumulator = 0;
                }
            }
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
        if (!_completed)
        {
            _charAccumulator += delta * _cps;
            int add = (int)_charAccumulator;
            if (add > 0)
            {
                _visibleChars = Math.Min(_text.Length, _visibleChars + add);
                _charAccumulator -= add;
                if (_visibleChars >= _text.Length)
                    _completed = true;
            }
        }
        else
        {
            // blink "continue" indicator
            _blinkTimer += delta;
            if (_blinkTimer >= 0.45)
            {
                _blinkTimer = 0;
                _blinkOn = !_blinkOn;
            }
        }
    }

    private void OnRender(double delta)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Dialogue panel
        _panel.DrawRect(_panelX, _panelY, _panelW, _panelH, 0.15f, 0.15f, 0.15f, 1f);

        // inner text box
        float textX = _panelX + _padL;
        float textY = _panelY + _padT;
        float maxWidth = _panelW - (_padL + _padR);

        _fontRenderer.DrawTextWrapped(
            _text,
            textX,
            textY,
            maxWidth,
            _textScale,
            1f, 1f, 1f, 1f,
            maxLines: 0,
            visibleChars: _visibleChars
        );

        // Continue indicator (use ">" because it's likely in your glyph set)
        if (_completed && _blinkOn)
        {
            float indX = _panelX + _panelW - _padR - 20;
            float indY = _panelY + _panelH - _padB - 30;
            _fontRenderer.DrawTextWrapped(">", indX, indY, 9999, _textScale, 1f, 1f, 1f, 1f);
        }
    }
}
