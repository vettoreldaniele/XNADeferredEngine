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
    public class DeferredRenderTarget
    {
        // G Buffer render targets.
        RenderTarget2D deferredDiffuseTarget;
        RenderTarget2D deferredNormalTarget;
        RenderTarget2D deferredDepthTarget;

        // Edges render target.
        RenderTarget2D deferredEdgeTarget;

        // Lights render target.
        public RenderTarget2D lightTarget { get; protected set; }


        public static RenderTarget2D blurTarget = EngineGlobals.shadowBlurType != ShadowBlurType.None ?
            new RenderTarget2D(Game1.graphics.GraphicsDevice, EngineGlobals.shadowMapSize, EngineGlobals.shadowMapSize, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.PreserveContents) : null;

        // Properties to access G buffer targets from other classes.
        public RenderTarget2D DeferredDiffuseTarget
        {
            get { return deferredDiffuseTarget; }
            protected set { }
        }

        public RenderTarget2D DeferredNormalTarget
        {
            get { return deferredNormalTarget; }
            protected set { }
        }

        public RenderTarget2D DeferredDepthTarget
        {
            get { return deferredDepthTarget; }
            protected set { }
        }

        public RenderTarget2D LightTarget
        {
            get { return lightTarget; }
            protected set { }
        }

        PresentationParameters pp;
        GraphicsDeviceManager deviceManager;

        public void LoadDeferredRenderTarget(Game game, QualityType quality, RenderingTechnique technique)
        {
            deviceManager = (GraphicsDeviceManager)game.Services.GetService(typeof(GraphicsDeviceManager));

            pp = deviceManager.GraphicsDevice.PresentationParameters;

            if (quality == QualityType.High) // If quality is set to high, use 64-bits render targets.
            {
                deferredDiffuseTarget = new RenderTarget2D(deviceManager.GraphicsDevice,
                    pp.BackBufferWidth, pp.BackBufferHeight, false,
                    SurfaceFormat.HalfVector4, DepthFormat.Depth24);

                deferredNormalTarget = new RenderTarget2D(deviceManager.GraphicsDevice,
                    pp.BackBufferWidth, pp.BackBufferHeight, false,
                    SurfaceFormat.HalfVector4, DepthFormat.None);

                deferredDepthTarget = new RenderTarget2D(deviceManager.GraphicsDevice,
                    pp.BackBufferWidth, pp.BackBufferHeight, false,
                    SurfaceFormat.HalfVector4, DepthFormat.None);
            }

            else // Use 32-bits render targets.
            {
                deferredDiffuseTarget = new RenderTarget2D(deviceManager.GraphicsDevice,
                    pp.BackBufferWidth, pp.BackBufferHeight, true,
                    SurfaceFormat.Color, DepthFormat.Depth24);

                deferredNormalTarget = new RenderTarget2D(deviceManager.GraphicsDevice,
                    pp.BackBufferWidth, pp.BackBufferHeight, true,
                    SurfaceFormat.Color, DepthFormat.None);

                deferredDepthTarget = new RenderTarget2D(deviceManager.GraphicsDevice,
                    pp.BackBufferWidth, pp.BackBufferHeight, true,
                    SurfaceFormat.HalfVector2, DepthFormat.None);
            }

            // Set up the light render target.
            lightTarget = new RenderTarget2D(deviceManager.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, true,
                SurfaceFormat.Color, DepthFormat.Depth24,0,RenderTargetUsage.PreserveContents);

            if (technique == RenderingTechnique.Toon)
            {
                deferredEdgeTarget = new RenderTarget2D(deviceManager.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, SurfaceFormat.HalfVector2, DepthFormat.None);
            }

        }

        public void SetGBuffer(QuadRenderer quadRenderer)
        {
            deviceManager.GraphicsDevice.SetRenderTargets(deferredDiffuseTarget, deferredNormalTarget, deferredDepthTarget);

            // Clear G Buffer
            deviceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            Effect clearEffect = Effects.clearGBuffer;

            quadRenderer.RenderFullScreenQuad(clearEffect);
        }

        public void SetLightTarget()
        {
            // Set the light render target and clear it to transparent black (with alpha = 0, because the alpha channel is used for the lightmap).
            deviceManager.GraphicsDevice.SetRenderTarget(lightTarget);

            // Set up additive blending.   
            deviceManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            DepthStencilState depthState = new DepthStencilState();
            depthState.DepthBufferEnable = true;
            depthState.DepthBufferWriteEnable = false;

            deviceManager.GraphicsDevice.DepthStencilState = depthState;
        }

        public void ClearLightTarget()
        {
            // Clear the light target to transparent black (with alpha = 0, because the alpha channel is used for the lightmap).
            deviceManager.GraphicsDevice.SetRenderTarget(lightTarget);
            deviceManager.GraphicsDevice.Clear(Color.Transparent);
        }


        public void BlurLightMap(QuadRenderer quadRenderer, Vector2 halfPixel)
        {

            Effect blurEffect = Effects.gaussBlurEffect;
            blurEffect.Parameters["halfPixel"].SetValue(halfPixel);

            Game1.graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Horizontal.
            Helpers.SetBlurEffectParameters(blurEffect, 1.0f / (float)LightTarget.Width, 0);

            Game1.graphics.GraphicsDevice.SetRenderTarget(blurTarget);
            Game1.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            blurEffect.Parameters["Texture"].SetValue(LightTarget);

            quadRenderer.RenderFullScreenQuad(blurEffect);

            // Vertical.
            Helpers.SetBlurEffectParameters(blurEffect, 0, 1.0f / (float)LightTarget.Height);

            Game1.graphics.GraphicsDevice.SetRenderTarget(LightTarget);
            blurEffect.Parameters["Texture"].SetValue(blurTarget);
            Game1.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            quadRenderer.RenderFullScreenQuad(blurEffect);
        }

        public void MakeEdges(QuadRenderer quadRenderer, Vector2 halfPixel)
        {
            // Set the edge render target.
            deviceManager.GraphicsDevice.SetRenderTarget(deferredEdgeTarget);
            deviceManager.GraphicsDevice.Clear(Color.Transparent);

            Effect effect = Effects.deferredEdgeEffect;
       
            // Get the inverted screen size (1 / width, 1 / height).
            Vector2 inverseScreenSize = new Vector2(1.0f / deferredDepthTarget.Width, 1.0f / deferredDepthTarget.Height);

            // Effects parameters.
            effect.Parameters["halfPixel"].SetValue(halfPixel);
            effect.Parameters["depthMap"].SetValue(DeferredDepthTarget);
            effect.Parameters["normalMap"].SetValue(DeferredNormalTarget);
            effect.Parameters["screenInverse"].SetValue(inverseScreenSize);

            deviceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            quadRenderer.RenderFullScreenQuad(effect);
          
        }


        public void CombineImage(QuadRenderer quadRenderer, Vector2 halfPixel)
        {
            Effect effect = Effects.combineEffect;

            // Effects parameters.
            effect.Parameters["colorMap"].SetValue(DeferredDiffuseTarget);
            effect.Parameters["lightMap"].SetValue(LightTarget);
            effect.Parameters["edgeMap"].SetValue(deferredEdgeTarget);

            effect.Parameters["normalMap"].SetValue(DeferredNormalTarget);

            effect.Parameters["halfPixel"].SetValue(halfPixel);

            // Toon shading ?
            bool toon = EngineGlobals.renderTechinque == RenderingTechnique.Toon;
            effect.Parameters["ToonShading"].SetValue(toon);

            deviceManager.GraphicsDevice.BlendState = BlendState.Opaque;
            deviceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            deviceManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            quadRenderer.RenderFullScreenQuad(effect);

            deviceManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void ResolveGBuffer()
        {
            deviceManager.GraphicsDevice.SetRenderTarget(null);
        }

        public void DebugDrawGBuffer(SpriteBatch spriteBatch)
        {
            /*
            System.IO.FileStream fileStream = new System.IO.FileStream("D:/tex1.jpeg", System.IO.FileMode.Create);
            deferredPositionLightTarget.SaveAsJpeg(fileStream, deferredPositionLightTarget.Width, deferredPositionLightTarget.Height);
            fileStream.Close(); */

            int screenWidth = deferredDiffuseTarget.Width;
            int screenHeight = deferredDiffuseTarget.Height;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            spriteBatch.Draw(deferredDiffuseTarget, new Rectangle(0, 0, screenWidth / 2, screenHeight / 2), Color.White);
            spriteBatch.Draw(deferredNormalTarget, new Rectangle(screenWidth / 2, 0, screenWidth / 2, screenHeight / 2), Color.White);
            spriteBatch.Draw(deferredDepthTarget, new Rectangle(0, screenHeight / 2, screenWidth / 2, screenHeight / 2), Color.White);
            spriteBatch.Draw(lightTarget, new Rectangle(screenWidth / 2, screenHeight / 2, screenWidth / 2, screenHeight / 2), Color.White);
         //   spriteBatch.Draw(deferredEdgeTarget, new Rectangle(screenWidth / 2, screenHeight / 2, screenWidth / 2, screenHeight / 2), Color.White);

            spriteBatch.End();

            // Restore DepthStencilState to default.
            deviceManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
