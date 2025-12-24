using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Input;
using System;

namespace Narraloom.Engine
{
    public class EngineApp
    {
        private IWindow _window;
        private GL _gl;
        private IInputContext _input;
        private IKeyboard _keyboard;
        private double _time;
        private int _frames;

        public void Run()
        {
            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
            options.Title = "Narraloom Player";

            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.Update += OnUpdate;
            _window.Closing += OnClose;

            _window.Run();
        }

        private void OnLoad()
        {
            _gl = GL.GetApi(_window);
            _gl.ClearColor(0.05f, 0.05f, 0.07f, 1.0f);

            _input = _window.CreateInput();
            _keyboard = _input.Keyboards[0];

            _keyboard.KeyDown += (keyboard, key, scancode) =>
            {
                if (key == Key.Escape)
                    _window.Close();
            };
        }

        private void OnUpdate(double delta)
        {
            // Game logic later
        }

        private void OnRender(double delta)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _frames++;
            _time += delta;

            if (_time >= 1.0)
            {
                _window.Title = $"Narraloom Player | FPS: {_frames}";
                _frames = 0;
                _time = 0;
            }
        }

        private void OnClose()
        {
            _gl?.Dispose();
        }
    }
}
