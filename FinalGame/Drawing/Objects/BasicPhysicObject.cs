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
using SkinnedModel;

namespace FinalGame
{
    public class BasicPhysicObject
    {
        // Fixed position.
        Vector3 fixedPos;

        protected Matrix[] transforms;

        // The model of the object.
        public Model model;

        // Scale.
        public Vector3 scale;

        // Texture of the model.
        public Texture2D modelTexture;

        // Name of the object.
        string name;

        // Physic Actor.
        public Actor actor;

        // The model from which the convex hull is generated.
        protected Model modelPhysx;

        #region Constructor

        /// <summary>
        /// Constructor for a basic physic object.
        /// </summary>
        /// <param name="m">Model to be drawn.</param>
        /// <param name="mPhysx">Model from which the physic convex hull is generated (use null if model is the same).</param>
        /// <param name="world">Matrix with transformation of the object.</param>
        /// <param name="weight">Mass of the object.</param>
        /// <param name="scale">Scale of the object.</param>
        /// <param name="name">Name of the object.</param>
        /// <param name="game">Instance of game.</param>
        public BasicPhysicObject(Model m, Model mPhysx, Matrix world, float weight, Vector3 scale, string name, Game game)
        {
            model = m;

            if (modelPhysx != null)
                modelPhysx = mPhysx;
            else modelPhysx = m;

            this.scale = scale;

            this.modelTexture = GetTextureFromModel(m);

            this.fixedPos = world.Translation;

            // Look up the bone transform matrices.
            transforms = new Matrix[model.Bones.Count];       
            model.CopyAbsoluteBoneTransformsTo(transforms);

            CreatePhysicActor(weight, scale, name, world.Translation, game);
        }

        #endregion

        #region Create physic actor from the model.

        protected void CreatePhysicActor(float weight, Vector3 scale, string name, Vector3 positionWorld, Game game)
        {
            ModelMeshPart mesh = modelPhysx.Meshes.First().MeshParts[0];

            Matrix[] transforms = new Matrix[modelPhysx.Bones.Count];
            modelPhysx.CopyAbsoluteBoneTransformsTo(transforms);

            // Gets the vertices from the mesh
            try
            {
                VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[mesh.NumVertices];
                mesh.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);
                //    VertexPositionColor[] vertices = new VertexPositionColor[mesh.MeshParts[0].NumVertices];
                //     mesh.VertexBuffer.GetData<VertexPositionColor>(vertices);


                // Allocate memory for the points and triangles
                var convexMeshDesc = new ConvexMeshDescription()
                {
                    PointCount = vertices.Length
                };
                convexMeshDesc.Flags |= ConvexFlag.ComputeConvex;
                convexMeshDesc.AllocatePoints<Vector3>(vertices.Length);

                // Write in the points and triangles
                // We only want the Position component of the vertex. Also scale down the mesh
                foreach (VertexPositionNormalTexture vertex in vertices)
                //    foreach (VertexPositionColor vertex in vertices)
                {
                    Vector3 position = Vector3.Transform(vertex.Position, Matrix.CreateScale(scale) * transforms[0]);

                    convexMeshDesc.PointsStream.Write(position);
                }

                // Cook to memory or to a file
                MemoryStream stream = new MemoryStream();
                //FileStream stream = new FileStream( @"Convex Mesh.cooked", FileMode.CreateNew );

                Cooking.InitializeCooking(new ConsoleOutputStream());
                Cooking.CookConvexMesh(convexMeshDesc, stream);
                Cooking.CloseCooking();


                stream.Position = 0;

                Game1 game1 = (Game1)game;

                ConvexMesh convexMesh = game1.core.CreateConvexMesh(stream);

                ConvexShapeDescription convexShapeDesc = new ConvexShapeDescription(convexMesh);
                

                ActorDescription actorDesc = new ActorDescription()
                {
                    BodyDescription = new BodyDescription(weight),
                    GlobalPose = Matrix.CreateTranslation(positionWorld).AsPhysX(),
                    Name = name
                };

              //  actorDesc.Shapes.Add(convexShapeDesc);

                actor = game1.scene.CreateActor(actorDesc);
                
               
                this.name = actor.Name;

            }
            catch
            {
                VertexPositionColor[] vertices = new VertexPositionColor[mesh.NumVertices];
                mesh.VertexBuffer.GetData<VertexPositionColor>(vertices);


                // Allocate memory for the points and triangles
                var convexMeshDesc = new ConvexMeshDescription()
                {
                    PointCount = vertices.Length
                };
                convexMeshDesc.Flags |= ConvexFlag.ComputeConvex;
                convexMeshDesc.AllocatePoints<Vector3>(vertices.Length);

                // Write in the points and triangles
                // We only want the Position component of the vertex. Also scale down the mesh

                foreach (VertexPositionColor vertex in vertices)
                {
                    Vector3 position = Vector3.Transform(vertex.Position, Matrix.CreateScale(scale) * transforms[0]);

                    convexMeshDesc.PointsStream.Write(position);
                }


                // Cook to memory or to a file
                MemoryStream stream = new MemoryStream();
                //FileStream stream = new FileStream( @"Convex Mesh.cooked", FileMode.CreateNew );

                Cooking.InitializeCooking(new ConsoleOutputStream());
                Cooking.CookConvexMesh(convexMeshDesc, stream);
                Cooking.CloseCooking();


                stream.Position = 0;

                Game1 game1 = (Game1)game;

                ConvexMesh convexMesh = game1.core.CreateConvexMesh(stream);

                ConvexShapeDescription convexShapeDesc = new ConvexShapeDescription(convexMesh);

                ActorDescription actorDesc = new ActorDescription()
                {
                    BodyDescription = new BodyDescription(weight),
                    GlobalPose = Matrix.CreateTranslation(positionWorld).AsPhysX(),
                    Name = name
                };
             //   actorDesc.Shapes.Add(convexShapeDesc);
                actorDesc.Shapes.Add(new BoxShapeDescription(1,1,1));
                actor = game1.scene.CreateActor(actorDesc);
            //    actor.Shapes[0].LocalPose = Matrix.Identity.AsPhysX();
                this.name = actor.Name;
            }

            finally
            {      
            }
            
        }

        #endregion

        #region Change effect with exception handling.
        protected static void ChangeEffectUsedByModel(Model model, Effect replacementEffect)
        {

            try
            {
                // Table mapping the original effects to our replacement versions.
                Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();

                foreach (ModelMesh mesh in model.Meshes)
                {

                    // Scan over all the effects currently on the mesh.
                    foreach (BasicEffect oldEffect in mesh.Effects)
                    {    

                        // If we haven't already seen this effect...
                        if (!effectMapping.ContainsKey(oldEffect))
                        {
                            // Make a clone of our replacement effect. We can't just use
                            // it directly, because the same effect might need to be
                            // applied several times to different parts of the model using
                            // a different texture each time, so we need a fresh copy each
                            // time we want to set a different texture into it.
                            Effect newEffect = replacementEffect.Clone();

                            // Copy across the texture from the original effect.
                            newEffect.Parameters["Texture"].SetValue(oldEffect.Texture);

                            // newEffect.Parameters["TextureEnabled"].SetValue(
                            //                                       oldEffect.TextureEnabled);

                            effectMapping.Add(oldEffect, newEffect);
                        }
                    }


                    // Now that we've found all the effects in use on this mesh,
                    // update it to use our new replacement versions.
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = effectMapping[meshPart.Effect];
                    }
                }
            }
            catch (System.InvalidCastException)
            {

            }

            finally
            {

            }
        }

        #endregion



        /// <summary>
        /// Update physic object.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            // DEBUGOPTION rotate model using R key.
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                this.actor.GlobalPose = this.actor.GlobalPose * Matrix.CreateRotationZ(MathHelper.ToRadians(5f)).AsPhysX();
            }

            // DEBUGOPTION add random force to model using U key.
            if (Keyboard.GetState().IsKeyDown(Keys.U))
            {
                this.actor.AddForce(RandomGen.RandomVector3(50, 50, 50).AsPhysX(), ForceMode.Impulse);
            }

            if (name == "Plane")
            {
                this.actor.GlobalPose = Matrix.CreateTranslation(fixedPos).AsPhysX();
            }
        }

        public virtual void PlayAnimation(string name1, string name2)
        {

        }

        private void DrawModel(Matrix world, string effectTechniqueName, Model model, Game game, DrawType drawType, Matrix? customView, Matrix? customProjection)
        {
            Camera camera = ((Game1)game).camera;

            Matrix view = customView != null ? customView.Value : camera.View;
            Matrix projection = customProjection != null ? customProjection.Value : camera.Projection;

            // Forward Rendering.
            if (drawType == DrawType.Edges || drawType == DrawType.Model)
            {
                // Get the graphics device.
                GraphicsDeviceManager graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(GraphicsDeviceManager));

                // Set suitable renderstates for drawing a 3D model.
                graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                Effect effect = Effects.toonShader;

                // Specify which effect technique to use.
                effect.CurrentTechnique = effect.Techniques[effectTechniqueName];

                // Draw the model.
                foreach (ModelMesh mesh in model.Meshes)
                {
                    Matrix localWorld = Matrix.CreateScale(scale) *
                               transforms[mesh.ParentBone.Index];
                    actor.GlobalPose.As<Matrix>();

                    // Default parameters...
                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        // Get the texture of this mesh part and set it to the Texture parameter of the effect.
                        // TODO: The textures should be saved in a list.
                        effect.Parameters["Texture"].SetValue(GetTextureFromMeshPart(meshPart));

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            // Vertex and index buffers.
                            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

                            // Draw the mesh part.
                            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                        }
                    }
                }
            }

            else if (drawType == DrawType.GBuffer) // Deferred rendering.
            {
                // Get the graphics device.
                GraphicsDeviceManager graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(GraphicsDeviceManager));

                // Set suitable renderstates for drawing a 3D model.
                graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                Effect effect;

                effect = Effects.gBufferEffect;
                effect.CurrentTechnique = effect.Techniques[effectTechniqueName];

                // Set the number of shades required for proper cel shading.
                effect.Parameters["NumberOfShades"].SetValue(1);

                // Get the textures associated to this model (Normal and Specular).
                // Normal is at [0] index, Specular is at [1].
                Texture[] textureCollection = model.Tag as Texture[];

                effect.Parameters["SpecularMap"].SetValue((Texture2D)textureCollection[1]);
                effect.Parameters["NormalMap"].SetValue((Texture2D)textureCollection[0]);


                effect.Parameters["farPlane"].SetValue(camera.FarClip);

                effect.Parameters["isLit"].SetValue(true);

                // Begin drawing.
                foreach (ModelMesh mesh in model.Meshes)
                {
                    // Get the world matrix (scale * position).
                    Matrix localWorld = transforms[mesh.ParentBone.Index] *
                             Matrix.CreateScale(scale) *
                          actor.GlobalPose.As<Matrix>();

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        // Add these triangles to the scene triangle count.
                        DebugGlobals.sceneTriangleCount += meshPart.PrimitiveCount * 3;

                        // Get the texture of this mesh part and set it to the Texture parameter of the effect.
                        effect.Parameters["Texture"].SetValue(GetTextureFromMeshPart(meshPart));

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            // Vertex and index buffers.
                            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

                            // Draw the mesh part.
                            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                        }
                    }
                }
            }

            else if (drawType == DrawType.ShadowMap) // Shadow mapping.
            {
                // Get the graphics device.
                GraphicsDeviceManager graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(GraphicsDeviceManager));

                // Renderstates are set in the light class.
                Effect effect = Effects.varianceShadowMappingEffect;
                effect.CurrentTechnique = effect.Techniques[effectTechniqueName];

                // Max distance.
                BaseLight light = ObjectManager.currentLight;
                if (light is SpotLight)
                {
                    effect.Parameters["maxLightDistance"].SetValue(((SpotLight)light).MaxDistance);
                }

                // Begin drawing.
                foreach (ModelMesh mesh in model.Meshes)
                {
                    // Get the world matrix (scale * position).
                    Matrix localWorld = transforms[mesh.ParentBone.Index] *
                             Matrix.CreateScale(scale) *
                          actor.GlobalPose.As<Matrix>();

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);



                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            // Vertex and index buffers.
                            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

                            // Draw the mesh part.
                            graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                        }
                    }
                }

            }
        }


        /// <summary>
        /// Draws the model using different effects and specifying custom view and projection matrices.
        /// </summary>
        /// <param name="drawType">Specify what to draw.</param>
        public virtual void Draw(Game game, DrawType drawType, Matrix? customView, Matrix? customProjection)
        {
            switch (drawType)
            {
                // Forward.
                case DrawType.Edges:
                    DrawModel(GetPos(), "NormalDepth", model, game, drawType, customView, customProjection);
                    break;

                case DrawType.Model:
                    DrawModel(GetPos(), "Toon", model, game, drawType, customView, customProjection);
                    break;

                // Deferred.
                case DrawType.GBuffer:
                    DrawModel(GetPos(), "DrawGBuffer", model, game, drawType, customView, customProjection);
                    break;

                // Shadow mapping.
                case DrawType.ShadowMap:
                    DrawModel(GetPos(), "ShadowMapDepth", model, game, drawType, customView, customProjection);
                    break;
            }
        }

        /// <summary>
        /// Draws the model using different effects.
        /// </summary>
        /// <param name="drawType">Specify what to draw.</param>
        public virtual void Draw(Game game, DrawType drawType)
        {
            Draw(game, drawType, null, null);
        }

        /// <summary>
        /// Get the texture from a model from the first mesh of it, if the texture is stored in a BasicEffect instance. If no BasicEffect instance is found, null is returned.
        /// </summary>
        public Texture2D GetTextureFromModel(Model model)
        {
            // Try to get the first effect in the first mesh of the model and cast it to BasicEffect, where the texture is stored.
            BasicEffect basEffect = model.Meshes[0].Effects[0] as BasicEffect;

            if (basEffect != null)
            {
                return basEffect.Texture;
            }
            else return null;
        }

        /// <summary>
        /// Get the texture from a mesh part, if the texture is stored in a BasicEffect instance. If no BasicEffect instance is found, null is returned.
        /// </summary>
        public Texture2D GetTextureFromMeshPart(ModelMeshPart meshPart)
        {
            // Try to get the effect of the mesh part and cast it to BasicEffect, where the texture is stored.
            BasicEffect basEffect = meshPart.Effect as BasicEffect;

            if (basEffect != null)
            {
                return basEffect.Texture;
            }
            else return null;
        }


        /// <summary>
        /// Get the current object position in the world.
        /// </summary>
        public virtual Matrix GetPos()
        {
            return actor.GlobalPose.As<Matrix>();
        }

        /// <summary>
        /// Get the object name.
        /// </summary>
        public virtual string Name()
        {
            return name;
        }

    }
}
