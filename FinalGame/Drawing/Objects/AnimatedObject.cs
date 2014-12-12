using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using SkinnedModel;

using PhysxEngine;
using StillDesign.PhysX;

namespace FinalGame
{
    public class AnimatedObject : BasicPhysicObject
    {
        SkinningData skinData;

        AnimationPlayer animPlayer;

        public AnimatedObject(Model m, Model mPhysx, Matrix world, float weight, Vector3 scale, string name, Game game) 
            : base (m,mPhysx,world,weight,scale,name,game)
            
        {
            skinData = m.Tag as SkinningData;
            if (skinData == null)
                throw new InvalidOperationException ("This model does not contain a SkinningData tag.");

            animPlayer = new AnimationPlayer(skinData);
        }

        public override void PlayAnimation(string name1, string name2)
        {         
            AnimationClip clip1 = skinData.AnimationClips[name1];
            AnimationClip clip2;
            if (name2 != null)
            {
               clip2 = skinData.AnimationClips[name2];
            }
            else
            {
                clip2 = null;
            }

            if (clip2 != null)
            {
                foreach (int index in clip1.BonesAnimated)
                {
                    foreach (int index2 in clip2.BonesAnimated)
                    {
                        if (index == index2)
                            throw new NotImplementedException("Animations must not move the same bones.");
                    }
                }

                if (clip1.Duration != clip2.Duration)
                    throw new NotImplementedException("Animations must have the same duration.");
            }

            animPlayer.StartClip(clip1, clip2);
        }

        public override void Update(GameTime gameTime)
        {
            animPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.CreateScale(scale) * actor.GlobalPose.As<Matrix>());

            base.Update(gameTime);
        }

        private void DrawModel(Matrix world, string effectTechniqueName, Model model, Game game, DrawType drawType, Matrix? customView, Matrix? customProjection)
        {
            Camera camera = ((Game1)game).camera;

            Matrix view = customView != null ? customView.Value : camera.View;
            Matrix projection = customProjection != null ? customProjection.Value : camera.Projection;

            if (drawType == DrawType.Edges || drawType == DrawType.Model)
            {
                GraphicsDeviceManager graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(GraphicsDeviceManager));

                // Set suitable renderstates for drawing a 3D model.
                graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;


                // Look up the bone transform matrices.
                Matrix[] transforms = new Matrix[model.Bones.Count];

                model.CopyAbsoluteBoneTransformsTo(transforms);

                Effect effect = Effects.skinnedEffect;

                Matrix[] bones = animPlayer.GetSkinTransforms();

                // Draw the model.
                foreach (ModelMesh mesh in model.Meshes)
                {

                    Matrix localWorld = Matrix.CreateScale(scale) *
                               transforms[mesh.ParentBone.Index] *
                              actor.GlobalPose.As<Matrix>();

                    // Default parameters...
                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);

                    // Skinned effect parameters.
                    effect.Parameters["Bones"].SetValue(bones);

                    //    graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap; // may be needed in some cases...

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
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
            else if (drawType == DrawType.GBuffer) // Deferred
            {
                // Get the graphics device.
                GraphicsDeviceManager graphics = (GraphicsDeviceManager)game.Services.GetService(typeof(GraphicsDeviceManager));

                // Set suitable renderstates for drawing a 3D model.
                graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default; ;

                Effect effect;

                effect = Effects.gBufferSkinnedEffect;
                effect.CurrentTechnique = effect.Techniques[effectTechniqueName];

                effect.Parameters["SpecularMap"].SetValue(skinData.Textures.SpecularMap);
                effect.Parameters["NormalMap"].SetValue(skinData.Textures.NormalMap);

                effect.Parameters["farPlane"].SetValue(camera.FarClip);

                // Get bones.
                effect.Parameters["Bones"].SetValue(animPlayer.GetSkinTransforms());

                // Begin drawing.
                foreach (ModelMesh mesh in model.Meshes)
                {
                    // Get the world matrix (scale * position).
                    Matrix localWorld = transforms[mesh.ParentBone.Index] * 
                        Matrix.CreateScale(scale) *             
                          actor.GlobalPose.As<Matrix>();

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);

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

                // Get bones.
                effect.Parameters["Bones"].SetValue(animPlayer.GetSkinTransforms());

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
        public override void Draw(Game game, DrawType drawType, Matrix? customView, Matrix? customProjection)
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
                    DrawModel(GetPos(), "DrawSkinnedGBuffer", model, game, drawType, customView, customProjection);
                    break;

                // Shadow mapping.
                case DrawType.ShadowMap:
                    DrawModel(GetPos(), "ShadowMapDepthSkinned", model, game, drawType, customView, customProjection);
                    break;
            }
        }

        /// <summary>
        /// Draws the model using different effects.
        /// </summary>
        /// <param name="drawType">Specify what to draw.</param>
        public override void Draw(Game game, DrawType drawType)
        {
            Draw(game, drawType, null, null);
        }

    }
}
