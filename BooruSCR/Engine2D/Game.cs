using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using TA.Engine2D;

namespace TA.Engine2D
{
    public abstract class Game : GameWindow
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0, 0, 0, 1);
            PreLoop();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            PostLoop();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Update(e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Draw(e.Time);
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1, 1);
            GL.Viewport(0, 0, Width, Height);
        }

        protected abstract void PreLoop();
        protected abstract void Update(double Elapsed);
        protected abstract void Draw(double Elapsed);
        protected abstract void PostLoop();
    }
}