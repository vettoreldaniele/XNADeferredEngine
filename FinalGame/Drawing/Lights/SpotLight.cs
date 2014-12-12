using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysxEngine;

namespace FinalGame
{
    class SpotLight : BaseLight
    {
        #region Fields and properties.

        private Vector3 direction;

        private float maxDistance;    

        private float decayRate;

        private float cosAngle;

        private float lightIntensity;

        private Matrix view = Matrix.Identity;

        private Matrix projection = Matrix.Identity;   

        /// <summary>
        /// Gets the projection matrix of the spot light.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            protected set { projection = value; }
        }

        /// <summary>
        /// Sets the projection matrix of the spot light.
        /// </summary>
        public Matrix View
        {
            get { return view; }
            protected set { view = value; }
        }

        /// <summary>
        /// Gets or sets the max distance that the spot light affects.
        /// </summary>
        public float MaxDistance
        {
            get { return maxDistance; }
            set
            {
                maxDistance = value;
                needUpdate = true;
            }
        }

        /// <summary>
        /// Gets or sets the intensity of the spot light.
        /// </summary>
        public float LightIntensity
        {
            get { return lightIntensity; }
            set { lightIntensity = value; }
        }

        /// <summary>
        /// Gets or sets the rate of decay of the spot light, which measures how the light intensity decreases from the center of the cone.
        /// </summary>
        public float DecayRate
        {
            get { return decayRate; }
            set { decayRate = value; }
        }

        /// <summary>
        /// Gets or sets the angle of the spot light in radians. Must be positive and between 0 and 90 degrees.
        /// </summary>
        public float Angle
        {
            get { return (float)Math.Acos(cosAngle); }
            set
            {
                if (MathHelper.ToDegrees(value) >= 0 && MathHelper.ToDegrees(value) <= 90)
                    cosAngle = (float)Math.Cos(value);
                else throw new ArgumentOutOfRangeException();

                needUpdate = true;
            }
        }

        /// <summary>
        /// Gets or sets the direction of the spot light.
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
                needUpdate = true;
            }
        }

        #endregion

        #region Constructors 

        /// <summary>
        /// Creates a new spot light, specyfing position, direction, maximum distance, angle, diffuse color, intensity, rate of decay and wheter the light cast shadows and can flicker.
        /// </summary>
        /// <param name="position">Position of the spot light.</param>
        /// <param name="radius">Radius of the spot light.</param>
        /// <param name="color">Diffuse color of the spot light.</param>
        /// <param name="maxDistance">Maximum distance the spot light affects.</param>
        /// <param name="angle">Angle of the spot light in radians. Must be in the 0..Pi/2 range.</param>
        /// <param name="intensity">The intensity of the spot light. Default is 1.0f.</param>
        /// <param name="intensity">The decay rate of the spot light. Default is 1.0f.</param>
        /// <param name="castShadows">True if the spot light casts shadows, false otherwise. (default true)</param>
        /// <param name="canFlicker">True if the spot light can flicker, false otherwise. (default false)</param>
        public SpotLight(Vector3 position, Vector3 direction, float maxDistance, float angle, Color color, float intensity = 1.0f, float decayRate = 1f, bool castShadows = true, bool canFlicker = false)
            : base(position, color, castShadows, canFlicker)
        {
            // If the angle is between 0 and 90 degrees, use it. Otherwise, set the angle at 90 degrees.
            if (MathHelper.ToDegrees(angle) <= 90 && MathHelper.ToDegrees(angle) >= 0)
            {
                this.Angle = angle;
            }
            else this.Angle = MathHelper.ToRadians(90f);

            this.direction = direction;

            this.maxDistance = maxDistance;

            this.decayRate = decayRate;

            this.lightIntensity = intensity;

            UpdateMatrices();
        }

        /// <summary>
        /// Creates a new spot light, specyfing position, direction, angle, intensity, rate of decay and wheter the light cast shadows and can flicker.
        /// </summary>
        /// <param name="position">Position of the spot light.</param>
        /// <param name="radius">Radius of the spot light.</param>
        /// <param name="maxDistance">Maximum distance the spot light affects.</param>
        /// <param name="angle">Angle of the spot light in radians. Must be in the 0..Pi/2 range.</param>
        /// <param name="intensity">The intensity of the spot light. Default is 1.0f.</param>
        /// <param name="intensity">The decay rate of the spot light. Default is 1.0f.</param>
        /// <param name="castShadows">True if the spot light casts shadows, false otherwise. (default true)</param>
        /// <param name="canFlicker">True if the spot light can flicker, false otherwise. (default true)</param>
        public SpotLight(Vector3 position, Vector3 direction, float maxDistance, float angle, float intensity = 1.0f, float decayRate = 1f, bool castShadows = true, bool canFlicker = false)
            : this(position,direction,maxDistance,angle,Color.White,intensity,decayRate,castShadows,canFlicker)
        {         
        }

        #endregion 

        public override void UpdateLight(GameTime gameTime)
        {
            UpdateMatrices();
        }

        public override void DrawShadowMap()
        {         
            Game1.graphics.GraphicsDevice.SetRenderTarget(ShadowMap);

            // Set renderstates.
            Game1.graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Game1.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Game1.objectManager.DrawScene(View, Projection);
        }


        public void UpdateMatrices()
        {
            if (needUpdate == true)
            {
                // Target
                Vector3 target = (Position + Direction);
                if (target == Vector3.Zero) target = -Vector3.Up;

                // Up
                Vector3 up = Vector3.Cross(direction, Vector3.Up);
                if (up == Vector3.Zero) up = Vector3.Right;
                else up = Vector3.Up;

                // Create view and projection matrices.
                View = Matrix.CreateLookAt(Position, target, up);
              
                Projection = Matrix.CreatePerspectiveFieldOfView(Angle, 1.0f, 1.0f, MaxDistance);

                // Reset boolean.
                needUpdate = false;
            }
        }

        public override void DrawLight(DeferredRenderTarget gBuffer, Game1 game, QuadRenderer quadRenderer, Vector2 halfPixel)
        {
        //   System.IO.FileStream fileStream = new System.IO.FileStream("D:/tex1.jpeg", System.IO.FileMode.Create);
        //   verticalBlur.SaveAsJpeg(fileStream, verticalBlur.Width, verticalBlur.Height);
        //   fileStream.Close(); 

        //    System.IO.FileStream fileStream = new System.IO.FileStream("D:/tex1.jpeg", System.IO.FileMode.Create);
        //    ShadowMap.SaveAsJpeg(fileStream, ShadowMap.Width, ShadowMap.Height);
       //     fileStream.Close(); 

            bool drawLight = true;

            if (CanFlicker)
            {
                // Probability that the light won't be drawn.
                drawLight = !RandomGen.CalculateProbability(LightGlobals.FlickerProbability);
            }

            if (drawLight == true)
            {

                // Get the point light effect.
                Effect effect = Effects.spotLightEffect;
                effect.CurrentTechnique = effect.Techniques["SpotLight"];

                // Set up effect parameters.
                effect.Parameters["colorMap"].SetValue(gBuffer.DeferredDiffuseTarget);
                effect.Parameters["normalMap"].SetValue(gBuffer.DeferredNormalTarget);
                effect.Parameters["depthMap"].SetValue(gBuffer.DeferredDepthTarget);

                // Light parameters.
                effect.Parameters["lightPos"].SetValue(Position);
                effect.Parameters["color"].SetValue(LightColor.ToVector3());
                effect.Parameters["lightDirection"].SetValue(Direction);
                effect.Parameters["cosLightAngle"].SetValue(cosAngle);
                effect.Parameters["spotDecayExponent"].SetValue(DecayRate);
                effect.Parameters["maxDistance"].SetValue(MaxDistance);
                effect.Parameters["lightIntensity"].SetValue(LightIntensity);

                effect.Parameters["lightViewProj"].SetValue(View * Projection);
                effect.Parameters["cookieTexture"].SetValue(LightCookies.spotRadialCookie);

                effect.Parameters["eyePosition"].SetValue(game.camera.Position);
                effect.Parameters["farClip"].SetValue(game.camera.FarClip);
                effect.Parameters["inverseView"].SetValue(Matrix.Invert(game.camera.View));
                effect.Parameters["inverseViewProj"].SetValue(Matrix.Invert(game.camera.View * game.camera.Projection));
                effect.Parameters["halfPixel"].SetValue(halfPixel);

                effect.Parameters["CastShadows"].SetValue(CastShadows && EngineGlobals.isWithShadows);

                if (CastShadows)
                {
                    effect.Parameters["shadowMap"].SetValue(ShadowMap);
                }


                Matrix coneMatrix;

                /*       
                Vector3 coneDirection = new Vector3(0,0,-1);
                float angle = (float)Math.Acos(Vector3.Dot(coneDirection,Direction));
                Vector3 axis = Vector3.Cross(coneDirection,Direction);

                float scale = (float)Math.Tan((double)Angle / 2.0) * 2 * MaxDistance;

                coneMatrix = Matrix.CreateScale(new Vector3(scale, scale, MaxDistance)) * Matrix.CreateFromAxisAngle(axis, angle) * Matrix.CreateTranslation(Position);  */


                //Make Scaling Factor
                float radial = (float)Math.Tan((double)Angle / 2.0) * 2 * MaxDistance;

                //Make Scaling Matrix
                Matrix Scaling = Matrix.CreateScale(radial, radial, MaxDistance);

                //Make Translation Matrix
                Matrix Translation = Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);

                //Make Inverse View
                Matrix inverseView = Matrix.Invert(View);

                //Make Semi-Product
                Matrix semiProduct = Scaling * inverseView;

                //Decompose Semi-Product
                Vector3 S; Vector3 P; Quaternion Q;
                semiProduct.Decompose(out S, out Q, out P);

                //Make Rotation
                Matrix Rotation = Matrix.CreateFromQuaternion(Q);

                //Make World
                coneMatrix = Scaling * Rotation * Translation;


                effect.Parameters["World"].SetValue(coneMatrix);
                effect.Parameters["View"].SetValue(game.camera.View);
                effect.Parameters["Projection"].SetValue(game.camera.Projection);


                //Calculate L
                Vector3 L = game.camera.Position - Position;

                //Calculate S.L
                float SL = Math.Abs(Vector3.Dot(L, Direction));

                //Check if SL is within the LightAngle, if so then draw the BackFaces, if not then draw the FrontFaces
                if (SL <= cosAngle) game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                else game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                Model model = LightVolumes.spotLightVolume;

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
