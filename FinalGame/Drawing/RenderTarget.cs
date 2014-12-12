using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalGame
{
    public class PublicRenderTarget
    {
        public RenderTarget2D sceneRenderTarget { get; protected set; }
        public RenderTarget2D normalDepthRenderTarget { get; protected set; }
        public PresentationParameters pp;

        public void LoadRenderTarget(Game game)
        {
            GraphicsDeviceManager graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(GraphicsDeviceManager));

            pp = graphics.GraphicsDevice.PresentationParameters;

            sceneRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, false,
                pp.BackBufferFormat,DepthFormat.Depth24);

            normalDepthRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, false,
                pp.BackBufferFormat, DepthFormat.Depth24);
        }

    }
}
