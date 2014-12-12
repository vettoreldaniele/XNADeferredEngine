using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FinalGame
{
    public class FrameRateCounter : DrawableGameComponent
    {
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;


        public FrameRateCounter(Game game)
            : base(game)
        {
            content = new ContentManager(game.Services);
            content.RootDirectory = "Content";
        }


        protected override void LoadContent()
        {                  
                spriteBatch = new SpriteBatch(GraphicsDevice);
                
                spriteFont = content.Load<SpriteFont>("Fonts/Arial");
            
        }


        protected override void UnloadContent()
        {
                content.Unload();
        }


        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            string fps = string.Format("FPS: {0}", frameRate);

            string nLights;

            string nTriangles = string.Format("Triangles: {0}", DebugGlobals.sceneTriangleCount);

            if (DebugGlobals.drawLights == true)
            {
                 nLights = string.Format("Lights: {0}", BaseLight.numberLights);
            }
            else  nLights = string.Format("Lights: {0}" + " (disabled)", BaseLight.numberLights);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            spriteBatch.DrawString(spriteFont, fps, new Vector2(33, 33), Color.Black);
            spriteBatch.DrawString(spriteFont, fps, new Vector2(32, 32), Color.White);
            spriteBatch.DrawString(spriteFont, nLights, new Vector2(33, 50), Color.Black);
            spriteBatch.DrawString(spriteFont, nLights, new Vector2(32, 49), Color.White);
            spriteBatch.DrawString(spriteFont, nTriangles, new Vector2(33, 70), Color.Black);
            spriteBatch.DrawString(spriteFont, nTriangles, new Vector2(32, 69), Color.White);

            spriteBatch.End();

            // Reset device states.
            GraphicsDevice.BlendState = BlendState.Opaque;


            // Reset globals.
            DebugGlobals.sceneTriangleCount = 0;

        }
    }
}