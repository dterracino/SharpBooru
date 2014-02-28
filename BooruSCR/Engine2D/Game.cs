using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using TA.Engine2D;

namespace TA.Engine2D
{
    public abstract class Game : GameWindow
    {
        /// <summary>DO NOT OVERLOAD THIS</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0, 0, 0, 1);
            PreLoop();
        }

        /// <summary>DO NOT OVERLOAD THIS</summary>
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            PostLoop();
        }

        /// <summary>DO NOT OVERLOAD THIS</summary>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Update(e.Time * 1000);
        }

        /// <summary>DO NOT OVERLOAD THIS</summary>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Draw(e.Time * 1000);
            SwapBuffers();
        }

        /// <summary>DO NOT OVERLOAD THIS</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1, 1);
            GL.Viewport(0, 0, Width, Height);
        }

        /// <summary>This method is called before the game loop starts (GL Context already created)</summary>
        protected abstract void PreLoop();

        /// <summary>This method is called before a frame is drawn</summary>
        /// <param name="Elapsed">The milliseconds elapsed since the last Update call</param>
        protected abstract void Update(double Elapsed);

        /// <summary>This method is called when a frame is drawn</summary>
        /// <param name="Elapsed">The milliseconds elapsed since the last Draw call</param>
        protected abstract void Draw(double Elapsed);

        /// <summary>This method is called when the game loop exits (GL Context not yet destroyed)</summary>
        protected abstract void PostLoop();
    }
}