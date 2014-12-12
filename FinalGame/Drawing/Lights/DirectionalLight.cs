using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysxEngine;
using System;

namespace FinalGame
{
    class DirectionalLight : BaseLight
    {
        #region Fields and Properties

        private Vector3 direction;

        /// <summary>
        /// Gets or sets the direction of the directional light.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return Vector3.Normalize(direction);
            }
            set
            {
                direction = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new directional light, specyfing position, direction, diffuse color, and wheter the light cast shadows and can flicker.
        /// </summary>
        /// <param name="position">Position of the directional light.</param>
        /// <param name="direction">Direction of the directional light.</param>
        /// <param name="color">Diffuse color of the directional light.</param>
        /// <param name="castShadows">True if the directional light casts shadows, false otherwise. (default true)</param>
        /// <param name="canFlicker">True if the directional light can flicker, false otherwise. (default false)</param>
        public DirectionalLight(Vector3 position, Vector3 direction, Color color, bool castShadows = true, bool canFlicker = false)
            : base(position, color, castShadows, canFlicker)
        {
            this.direction = direction;
        }

        /// <summary>
        /// Creates a new directional light with white diffuse color, specyfing position, direction, and wheter the light cast shadows and can flicker.
        /// </summary>
        /// <param name="position">Position of the directional light.</param>
        /// <param name="direction">Direction of the directional light.</param>
        /// <param name="color">Diffuse color of the directional light.</param>
        /// <param name="castShadows">True if the directional light casts shadows, false otherwise. (default true)</param>
        /// <param name="canFlicker">True if the directional light can flicker, false otherwise. (default false)</param>
        public DirectionalLight(Vector3 position, Vector3 direction, bool castShadows = true, bool canFlicker = false)
            : this(position, direction, Color.White, castShadows, canFlicker)
        {
        }

        #endregion 

        public override void UpdateLight(GameTime gameTime)
        {

        }

        public override void DrawShadowMap()
        {
           
        }


        public override void DrawLight(DeferredRenderTarget gBuffer, Game1 game, QuadRenderer quadRenderer, Vector2 halfPixel)
        {
            bool drawLight = true;

            if (CanFlicker)
            {
                // Probability that the light won't be drawn.
                drawLight = !RandomGen.CalculateProbability(LightGlobals.FlickerProbability);
            }

            if (drawLight == true)
            {

                // Get the directional light effect.
                Effect effect = Effects.directionalLightEffect;
                effect.CurrentTechnique = effect.Techniques["DirectionalLight"];

                // Set up effect parameters.
                effect.Parameters["colorMap"].SetValue(gBuffer.DeferredDiffuseTarget);
                effect.Parameters["normalMap"].SetValue(gBuffer.DeferredNormalTarget);
                effect.Parameters["depthMap"].SetValue(gBuffer.DeferredDepthTarget);
                effect.Parameters["lightDirection"].SetValue(Direction);
                effect.Parameters["lightColor"].SetValue(LightColor.ToVector3());
                effect.Parameters["eyePosition"].SetValue(game.camera.Position);
                effect.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(game.camera.View * game.camera.Projection));
                effect.Parameters["halfPixel"].SetValue(halfPixel);


                Vector3[] corners;
                GetFrustumCorners(game.camera, out corners);

                effect.Parameters["frustumCorners"].SetValue(corners);
                effect.Parameters["camWorld"].SetValue(Matrix.Invert(game.camera.View));

                // Apply the effect to a fullscreen quad, that is the whole screen as the directional light affects all the pixels.
                quadRenderer.RenderFullScreenQuad(effect);

            }
        }

        private void GetFrustumCorners(Camera camera, out Vector3[] corners)
        {
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            corners = new Vector3[4];

            Vector3[] temp = frustum.GetCorners();
            for (int i = 0; i < 4; i++)
            {
                corners[i] = Vector3.Transform(temp[i + 4], camera.View);
            }
        }
    }
}
