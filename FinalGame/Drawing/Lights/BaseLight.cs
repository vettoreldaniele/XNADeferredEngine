using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalGame
{
    abstract public class BaseLight
    {
        #region Fields and Properties

        public static int numberLights = 0;

        private bool castShadows;

        private static RenderTarget2D shadowMap = new RenderTarget2D(Game1.graphics.GraphicsDevice, EngineGlobals.shadowMapSize, EngineGlobals.shadowMapSize, true, SurfaceFormat.HalfVector2, DepthFormat.Depth24);

        // Boolean to check if the light view and projection matrices need to be updated.
        protected bool needUpdate;

        /// <summary>
        /// Gets or sets a value that indicates wheter the light cast shadows.
        /// </summary>
        public RenderTarget2D ShadowMap
        {
            get { return shadowMap; }
            protected set { }
        }

        /// <summary>
        /// Gets or sets a value that indicates wheter the light cast shadows.
        /// </summary>
        public bool CastShadows
        {
            get
            {
                return castShadows;
            }
            set
            {
                castShadows = value;
                if (value == false)
                {
                    shadowMap.Dispose();
                    shadowMap = null;
                }
            }
        }

        private bool canFlicker;

        /// <summary>
        /// Gets or sets a value that indicates wheter the light can flickers.
        /// </summary>
        public bool CanFlicker
        {
            get
            {
                return canFlicker;
            }
            set
            {
                canFlicker = value;
            }
        }

        private Vector3 position;

        /// <summary>
        /// Gets or sets the position of the light.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                needUpdate = true;
            }
        }

        private Color lightColor;

        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        public Color LightColor
        {
            get
            {
                return lightColor;
            }
            set
            {
                lightColor = value;
            }
        }

        #endregion

        public BaseLight(Vector3 pos, Color color, bool castShadows, bool canFlicker)
        {
            this.position = pos;
            this.castShadows = castShadows;
            this.canFlicker = canFlicker;
            this.lightColor = color;
            numberLights++;

        }

        /// <summary>
        /// Draws the light.
        /// </summary>
        public abstract void DrawLight(DeferredRenderTarget gBuffer, Game1 game, QuadRenderer quadRenderer, Vector2 halfPixel);

        /// <summary>
        /// Updates the light.
        /// </summary>
        public abstract void UpdateLight(GameTime gameTime);

        /// <summary>
        /// Draws the shadow map of the light into the shadow map render target.
        /// </summary>
        public abstract void DrawShadowMap();
    }
}
