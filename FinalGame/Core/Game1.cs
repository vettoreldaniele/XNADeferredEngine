using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using System.Threading;
using System.Diagnostics;

namespace FinalGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        // Graphics and SpriteBatch.
        public static GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Camera.
        public Camera camera;

        // ObjectManager.
        public static ObjectManager objectManager;

        // Framerate Counter.
        FrameRateCounter fpsCounter;

        /// <summary>
        /// Physics engine.
        /// </summary>
        public Engine engine;
        /// <summary>
        /// Physics core.
        /// </summary>
        public Core core;
        /// <summary>
        /// Physics scene.
        /// </summary>
        public Scene scene;

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

        public Game1()
        {
            engine = new Engine(this);

            graphics = engine.DeviceManager;
            Services.AddService(typeof(GraphicsDeviceManager), graphics);
            Content.RootDirectory = "Content";

         //   graphics.PreferMultiSampling = true;

            // DEBUGOPTION Fullscreen.
            graphics.IsFullScreen = true;
            

            // DEBUGOPTION No Vsync.
            //     this.IsFixedTimeStep = false;
//             graphics.SynchronizeWithVerticalRetrace = false;

            // Resolution.
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
        //    graphics.PreferredBackBufferWidth = 1680;
        //    graphics.PreferredBackBufferHeight = 1050;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Initialize physics engine.
            engine.Initialize();
            core = engine.Core;
            scene = engine.Scene;
            camera = engine.Camera;

            // Remove ground actor.
            //   scene.Actors[0].Dispose();

            // Create and add the ObjectManager and the framerate counter.
            objectManager = new ObjectManager(this);
            Components.Add(objectManager);

            fpsCounter = new FrameRateCounter(this);
            Components.Add(fpsCounter);

            // Reference a static content manager and load all effects, light volumes and light cookies.
            Effects.manager = this.Content;
            Effects.LoadEffects();

            LightVolumes.manager = this.Content;
            

            LightCookies.manager = this.Content;
         

            base.Initialize();


        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures, and add it to the Game Services.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            LightVolumes.LoadLightVolumes();
            LightCookies.LoadCookies();

            GC.Collect();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // Update keyboard.
            currentKeyboardState = Keyboard.GetState();

            // Exit the game immediately if the user presses the Escape key.
            if (CheckForExit(currentKeyboardState))
            {
                base.Update(gameTime);
                core.Dispose();
                return;
            }

            // Update physics engine.
            engine.Update(gameTime);
         

            oldKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        // Allows the game to exit.
        protected bool CheckForExit(KeyboardState keyboard)
        {

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            engine.Device.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
