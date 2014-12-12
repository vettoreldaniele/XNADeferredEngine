using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalGame
{
    class PointLight : BaseLight
    {
        #region Fields and Properties

        private float radius;

        private float lightIntensity;

        /// <summary>
        /// Gets or sets the intensity of the point light.
        /// </summary>
        public float LightIntensity
        {
            get { return lightIntensity; }
            set { lightIntensity = value; }
        }

        /// <summary>
        /// Gets or sets the radius of the point light. Must be positive.
        /// </summary>
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (radius >= 0)
                radius = value;
                else throw new ArgumentOutOfRangeException();
            }
        }  

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new point light, specyfing position, radius, diffuse color, intensity, and wheter the light cast shadows and can flicker.
        /// </summary>
        /// <param name="position">Position of the point light.</param>
        /// <param name="radius">Radius of the point light.</param>
        /// <param name="color">Diffuse color of the point light.</param>
        /// <param name="intensity">The intensity of the point light. Default is 1.0f.</param>
        /// <param name="castShadows">True if the point light casts shadows, false otherwise. (default true)</param>
        /// <param name="canFlicker">True if the point light can flicker, false otherwise. (default false)</param>
        public PointLight(Vector3 position, float radius, Color color, float intensity = 1.0f, bool castShadows = true, bool canFlicker = false)
            : base(position, color, castShadows, canFlicker)
        {
            this.radius = radius;
            this.lightIntensity = intensity;
        }

        /// <summary>
        /// Creates a new point light with white diffuse color, specyfing position, radius, intensity and wheter the light cast shadows and can flicker.
        /// </summary>
        /// <param name="position">Position of the point light.</param>
        /// <param name="radius">Radius of the point light.</param>
        /// <param name="intensity">The intensity of the point light. Default is 1.0f.</param>
        /// <param name="castShadows">True if the point light casts shadows, false otherwise.</param>
        /// <param name="canFlicker">True if the point light can flicker, false otherwise.</param>
        public PointLight(Vector3 position, float radius, float intensity = 1.0f, bool castShadows = true, bool canFlicker = false)
            : this(position, radius, Color.White, intensity, castShadows, canFlicker)
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
                // Get the point light effect.
                Effect effect = Effects.pointLightEffect;
                effect.CurrentTechnique = effect.Techniques["PointLight"];

                // Set up effect parameters.
                effect.Parameters["colorMap"].SetValue(gBuffer.DeferredDiffuseTarget);
                effect.Parameters["normalMap"].SetValue(gBuffer.DeferredNormalTarget);
                effect.Parameters["depthMap"].SetValue(gBuffer.DeferredDepthTarget);

                // Light parameters.
                effect.Parameters["lightPos"].SetValue(Position);
                effect.Parameters["color"].SetValue(LightColor.ToVector3());
                effect.Parameters["radius"].SetValue(Radius);
                effect.Parameters["lightIntensity"].SetValue(LightIntensity);

                effect.Parameters["eyePosition"].SetValue(game.camera.Position);
                effect.Parameters["farClip"].SetValue(game.camera.FarClip);
                effect.Parameters["inverseView"].SetValue(Matrix.Invert(game.camera.View));
                effect.Parameters["inverseViewProj"].SetValue(Matrix.Invert(game.camera.View * game.camera.Projection));
                effect.Parameters["halfPixel"].SetValue(halfPixel);

                Matrix sphereMatrix = Matrix.CreateScale(radius) * Matrix.CreateTranslation(Position);
                effect.Parameters["World"].SetValue(sphereMatrix);
                effect.Parameters["View"].SetValue(game.camera.View);
                effect.Parameters["Projection"].SetValue(game.camera.Projection);

                // If the camera is inside the light volume, draw with CullMode.CullClockwise, else draw with CullMode.CullCounterClockwise.

                // Calculate the distance between the camera and light center
                float cameraToCenter = Vector3.Distance(game.camera.Position, Position);

                if (cameraToCenter < Radius)
                    game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                else
                    game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                Model model = LightVolumes.pointLightVolume;

                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            // Vertex and index buffers.
                            game.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                            game.GraphicsDevice.Indices = meshPart.IndexBuffer;

                            // Draw the mesh part.
                            game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                        }
                    }
                }

                // Reset the cull mode.
                game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

        }

    }
}
