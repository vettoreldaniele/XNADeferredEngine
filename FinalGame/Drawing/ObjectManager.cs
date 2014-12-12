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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using PhysxEngine;
using StillDesign.PhysX;

namespace FinalGame
{
    public class ObjectManager : DrawableGameComponent
    {
        // List of all objects.
        List<BasicPhysicObject> physicObjects = new List<BasicPhysicObject>();
        
        // List of lights.
        List<BaseLight> lights = new List<BaseLight>();

        // Current light.
        public static BaseLight currentLight;

        // Spritefont.
        SpriteFont font;

        // Render Target.
        PublicRenderTarget renderTarget;

        // Multiple Render Targets (MRT) for deferred rendering.
        DeferredRenderTarget deferredTargets;

        // Quad renderer.
        QuadRenderer quadRenderer;

        // Half of a pixel.
        private Vector2 halfPixel;

        private KeyboardState currentKeyboardState;
        private KeyboardState oldKeyboardState;

        public KeyboardState CurrentKeyboardState
        {
            get { return currentKeyboardState; }
            set { currentKeyboardState = value; }
        }

        public KeyboardState OldKeyboardState
        {
            get { return oldKeyboardState; }
            set { oldKeyboardState = value; }
        }

        public ObjectManager(Game game)
            : base(game)
        {

            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // Set engine globals.
            SetGlobalVariables();

            if (EngineGlobals.renderMode == RenderingMode.Forward)
            {
                // Render targets for forward rendering.
                renderTarget = new PublicRenderTarget();
                renderTarget.LoadRenderTarget(Game);
            }
            else
            {
                // Render targets for deferred rendering.
                deferredTargets = new DeferredRenderTarget();
                deferredTargets.LoadDeferredRenderTarget(Game, QualityType.Medium, EngineGlobals.renderTechinque);
            }

            // Load up the quad renderer.
            quadRenderer = new QuadRenderer((Game1)Game);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Font used to draw strings.
            font = Game.Content.Load<SpriteFont>("Fonts/Arial");

            // Calculate half of a pixel to correctly align texture and screen space coordinates.
            halfPixel.X = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferHeight;

            #region Objects

            Model model = Game.Content.Load<Model>("Models/ship1");
            Model model2 = Game.Content.Load<Model>("Models/dude");
            Model plane = Game.Content.Load<Model>("Models/ground");

            Model animModel = Game.Content.Load<Model>("Models/Tutorial_body14_anim");

           physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(0, 1, 0)), 10f, new Vector3(0.1f), "Plane", Game));
      //      physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(10, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(plane, null, Matrix.CreateTranslation(new Vector3(0, -10, 0)), 10f, new Vector3(1f, 1f, 1f), "Plane", Game));
        //    physicObjects.Add(new AnimatedObject(model2, null, Matrix.CreateTranslation(new Vector3(0, 30, 0)), 10f, new Vector3(0.1f), "Model", Game));
            //   physicObjects[0].PlayAnimation("WaveRight", "WaveLeft");
         //        physicObjects[0].PlayAnimation("Animation",null);

            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(20, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(15, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(25, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(30, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(35, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(45, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(40, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));
            physicObjects.Add(new BasicPhysicObject(model2, null, Matrix.CreateTranslation(new Vector3(50, 10, 0)), 10f, new Vector3(0.1f), "Ship", Game));  


            #endregion

            #region Lights

            Vector3 lightDirection = new Vector3(0, -1, 0);

            //    lights.Add(new PointLight(new Vector3(0, 15, 0), 30, Color.White, 1f));
            // lights.Add(new PointLight(new Vector3(10, 10, 0), 45f, Color.Yellow, 1f));
            //  lights.Add(new PointLight(new Vector3(-5, 10, 0), 45f, Color.Red, 1.3f));
            //   lights.Add(new PointLight(new Vector3(0, 7, 7), 45f, Color.Purple, 1.2f));
            //    lights.Add(new PointLight(new Vector3(0, 0, 0), 35f, Color.Red, 1f, true, false));
            //  lights.Add(new DirectionalLight(Vector3.Zero, new Vector3(0, -1, -1), Color.Wheat, true, false));
            //   lights.Add(new DirectionalLight(Vector3.Zero, lightDirection, Color.Yellow, true, false));
            lights.Add(new SpotLight(new Vector3(0, 15, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 1f, 1f, true, false));
          //  lights.Add(new SpotLight(new Vector3(0, 15, 20), Vector3.Normalize(new Vector3(0, -1, -1)), 100, MathHelper.ToRadians(30f), 0.5f, 1f, false, false));
            lights.Add(new SpotLight(new Vector3(10, 15, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.5f, 1f, true, false));

            lights.Add(new SpotLight(new Vector3(20, 20, 0), Vector3.Normalize(new Vector3(0, -1, -1)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(25, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(30, 20, 0), Vector3.Normalize(new Vector3(-1, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(40, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(45, 20, 0), Vector3.Normalize(new Vector3(0, -1, -1)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));
            lights.Add(new SpotLight(new Vector3(50, 20, 0), Vector3.Normalize(new Vector3(0, -1, 0)), 100, MathHelper.ToRadians(90f), 0.2f, 1f, true, false));

            #endregion

            GC.Collect();

            base.LoadContent();
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            currentKeyboardState = Keyboard.GetState();

            // Update all objects.
            for (int i = 0; i < physicObjects.Count; i++)
            {
                physicObjects[i].Update(gameTime);
            }

            // Update all lights.
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].UpdateLight(gameTime);
            }

          //  physicObjects[0].actor.GlobalPose = Matrix.CreateTranslation(new Vector3(0, 0, 0)).AsPhysX();
            
            // Process input.
            ProcessInput();

            oldKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        public void DrawScene(Matrix view, Matrix projection)
        {
            foreach (BasicPhysicObject phyObj in physicObjects)
            {
                phyObj.Draw(Game, DrawType.ShadowMap, view, projection);
            }
        }

        /// <summary>
        /// Allows the game component to draw objects.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            // Debug Option.
            // GraphicsDevice.Clear(Color.Red);

            // Get Graphics Device and SpriteBatch.
            GraphicsDeviceManager graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(GraphicsDeviceManager));
            GraphicsDevice device = graphics.GraphicsDevice;
            SpriteBatch spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            if (EngineGlobals.renderMode == RenderingMode.Forward)
            {
                // Draw Edges in the normalDepth Render Target.
                device.SetRenderTarget(renderTarget.normalDepthRenderTarget);
                device.Clear(Color.Black);
                foreach (BasicPhysicObject phyObj in physicObjects)
                {
                    phyObj.Draw(Game, DrawType.Edges);
                }

                // Draw Models in the Scene Render Target.           
                device.SetRenderTarget(renderTarget.sceneRenderTarget);
                device.Clear(Color.CornflowerBlue);
                foreach (BasicPhysicObject phyObj in physicObjects)
                {
                    phyObj.Draw(Game, DrawType.Model);
                }

                // Draw physics engine debug view.
                ((Game1)Game).engine.Draw(renderTarget.sceneRenderTarget);

                // Apply post process.
                device.SetRenderTarget(null);
                ApplyPostprocess(Game, renderTarget);
            }

            else // Deferred rendering
            {
                // Set the G Buffer and clear it, then draw the scene to it.
                deferredTargets.SetGBuffer(quadRenderer);

                foreach (BasicPhysicObject phyObj in physicObjects)
                {
                    phyObj.Draw(Game, DrawType.GBuffer);
                }

                // Resolve G Buffer.
                deferredTargets.ResolveGBuffer();

                if (EngineGlobals.renderTechinque == RenderingTechnique.Toon)
                {
                    deferredTargets.MakeEdges(quadRenderer, halfPixel);
                }

                deferredTargets.ClearLightTarget();

                foreach (BaseLight light in lights)
                {
                    currentLight = light;

                    if (EngineGlobals.isWithShadows)
                    {
                        if (light.CastShadows)
                        {
                            light.DrawShadowMap();                 
                        }
                    }

                    // Draw lights in the light render target with color blending enabled.
                    deferredTargets.SetLightTarget();

                    if (DebugGlobals.drawLights == true)
                    {
                        light.DrawLight(deferredTargets, (Game1)Game, quadRenderer, halfPixel);
                    }

                }

                // Set the device back to the backbuffer.
                device.SetRenderTarget(null);
                device.Clear(Color.CornflowerBlue);

                if (DebugGlobals.drawPhysxDebug)
                {
                    ((Game1)Game).engine.Draw(null);
                }

                deferredTargets.BlurLightMap(quadRenderer, halfPixel);

                device.SetRenderTarget(null);

                deferredTargets.CombineImage(quadRenderer, halfPixel);

                // Debug G Buffer.
                if (DebugGlobals.debugGBuffer)
                {
                    deferredTargets.DebugDrawGBuffer((SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)));
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Helper applies the edge detection for forward rendering.
        /// </summary>
        public void ApplyPostprocess(Game game, PublicRenderTarget renderTarget)
        {
            Effect postprocessEffect = Effects.postProcess;

            EffectParameterCollection parameters = postprocessEffect.Parameters;
            string effectTechniqueName;

            // Set effect parameters controlling the edge detection effect.

            Vector2 resolution = new Vector2(renderTarget.sceneRenderTarget.Width,
                                             renderTarget.sceneRenderTarget.Height);
            Texture2D normalDepthTexture = renderTarget.normalDepthRenderTarget;

            parameters["EdgeWidth"].SetValue(1);
            parameters["EdgeIntensity"].SetValue(1);
            parameters["ScreenResolution"].SetValue(resolution);
            parameters["NormalDepthTexture"].SetValue(normalDepthTexture);

            effectTechniqueName = "EdgeDetect";

            // Activate the appropriate effect technique.
            postprocessEffect.CurrentTechnique =
                                    postprocessEffect.Techniques[effectTechniqueName];

            // Retrieve SpriteBatch.
            SpriteBatch spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

            // Draw a fullscreen sprite to apply the postprocessing effect.

            // spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,SamplerState.LinearWrap,DepthStencilState.Default,RasterizerState.CullNone,postprocessEffect);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            postprocessEffect.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(renderTarget.sceneRenderTarget, Vector2.Zero, Color.White);

            spriteBatch.End();
        }

        private void ProcessInput()
        {
            // Debug mode input.
            if (EngineGlobals.isDebug == true)
            {
                if (currentKeyboardState.IsKeyDown(Keys.L) && oldKeyboardState.IsKeyUp(Keys.L))
                {
                    // Toggle lights drawing.
                    DebugGlobals.drawLights = !DebugGlobals.drawLights;
                }

                if (currentKeyboardState.IsKeyDown(Keys.O) && oldKeyboardState.IsKeyUp(Keys.O))
                {
                    // Toggle G Buffer debug.
                    DebugGlobals.debugGBuffer = !DebugGlobals.debugGBuffer;
                }

                if (currentKeyboardState.IsKeyDown(Keys.N) && oldKeyboardState.IsKeyUp(Keys.N))
                {
                    Dictionary<string, object> overrideFields = new Dictionary<string, object>();
                    overrideFields.Add("canFlicker", false);

                    // Adds a random directional light to the scene.
                    DirectionalLight light = new DirectionalLight(Vector3.Zero, Vector3.Zero, true, false);
                    RandomGen.MakeRandomClass(light,overrideFields);
                    lights.Add(light);
                }

                if (currentKeyboardState.IsKeyDown(Keys.C) && oldKeyboardState.IsKeyUp(Keys.C))
                {
                    // Clear the lights list.
                    lights.Clear();
                    BaseLight.numberLights = 0;
                }
                
                if (currentKeyboardState.IsKeyDown(Keys.T) && oldKeyboardState.IsKeyUp(Keys.T))
                {
                    // Toggle rendering technique.
                    if (EngineGlobals.renderTechinque == RenderingTechnique.Realistic)
                    {
                        EngineGlobals.renderTechinque = RenderingTechnique.Toon;
                    }
                    else EngineGlobals.renderTechinque = RenderingTechnique.Realistic;            
                }

                if (currentKeyboardState.IsKeyDown(Keys.K) && oldKeyboardState.IsKeyUp(Keys.K))
                {
                    // Toggle shadows.
                    EngineGlobals.isWithShadows = !EngineGlobals.isWithShadows;
                }

                if (currentKeyboardState.IsKeyDown(Keys.M) && oldKeyboardState.IsKeyUp(Keys.M))
                {
                    // Toggle shadow maps blurring.
                    var maxValue = (int)Enum.GetValues(typeof(ShadowBlurType)).Cast<ShadowBlurType>().Max();

                    int x = (int)EngineGlobals.shadowBlurType;

                    x = x != maxValue ? ++x : 0;

                    EngineGlobals.shadowBlurType = (ShadowBlurType)x;
                }

                if (currentKeyboardState.IsKeyDown(Keys.P) && oldKeyboardState.IsKeyUp(Keys.P))
                {      
                    // Toggle physX debug view.
                    DebugGlobals.drawPhysxDebug = !DebugGlobals.drawPhysxDebug;
                }
            }
        }

        private void SetGlobalVariables()
        {
            EngineGlobals.isDebug = true;

            EngineGlobals.renderMode = RenderingMode.Deferred;
            EngineGlobals.renderTechinque = RenderingTechnique.Toon;

            EngineGlobals.isWithShadows = true;
            EngineGlobals.shadowBlurType = ShadowBlurType.Gaussian15;
            EngineGlobals.shadowMapSize = 1024;          
        }

    }
}
