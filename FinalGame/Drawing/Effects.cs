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
using SkinnedModel;

using StillDesign.PhysX;

namespace FinalGame
{
    public class Effects
    {
        public static ContentManager manager;

        public static Effect toonShader;
        public static Effect postProcess;
        public static Effect skinnedEffect;
        public static Effect clearGBuffer;
        public static Effect gBufferEffect;
        public static Effect gBufferSkinnedEffect;
        public static Effect directionalLightEffect;
        public static Effect pointLightEffect;
        public static Effect spotLightEffect;
        public static Effect combineEffect;
        public static Effect deferredEdgeEffect;

        public static Effect gaussBlurEffect;

        public static Effect varianceShadowMappingEffect;

        public static void LoadEffects()
        {
            toonShader = manager.Load<Effect>("Effects/CartoonEffect");
            postProcess = manager.Load<Effect>("Effects/PostprocessEffect");
            skinnedEffect = manager.Load<Effect>("Effects/SkinnedModel");
            clearGBuffer = manager.Load<Effect>("Effects/ClearGBuffer");
            gBufferEffect = manager.Load<Effect>("Effects/GBufferEffect");
            gBufferSkinnedEffect = manager.Load<Effect>("Effects/GBufferSkinned");
            directionalLightEffect = manager.Load<Effect>("Effects/DirectionalLight");
            pointLightEffect = manager.Load<Effect>("Effects/PointLight");
            spotLightEffect = manager.Load<Effect>("Effects/SpotLight");
            combineEffect = manager.Load<Effect>("Effects/CombineDeferred");
            deferredEdgeEffect = manager.Load<Effect>("Effects/DeferredEdgeDetection");

            gaussBlurEffect = manager.Load<Effect>("Effects/GaussianBlur15");

            varianceShadowMappingEffect = manager.Load<Effect>("Effects/VarianceShadowMapping");
        }
    }
}
