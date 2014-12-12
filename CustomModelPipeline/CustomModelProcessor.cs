using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace CustomModelPipeline
{
    /// <summary>
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "Custom Model")]
    public class CustomModelProcessor : ModelProcessor
    {
        [DefaultValue("")]
        [DisplayName("Effects Path")]
        [Description("The path of the effects of the model, null if the effects are in the same folder.")]
        public string EffectPath
        {
            get { return effectPath; }
            set { effectPath = value; }
        }

        string effectPath;

        string[] effectsName = new string[] { "CartoonEffect", "GBufferEffect", "PostprocessEffect", "SkinnedModel" };

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {

            ModelContent model = base.Process(input, context);
            return model;
        }

        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            EffectMaterialContent newEffect = new EffectMaterialContent();

            foreach (string effectName in effectsName)
            {
                if (effectPath == "")
                {
                    effectPath = Path.GetFullPath(effectName + ".fx");
                    newEffect.Effect = new ExternalReference<EffectContent>(effectPath);
                }
                else
                {
                    newEffect.Effect = new ExternalReference<EffectContent>(effectPath);
                }
            }

            return base.ConvertMaterial(newEffect, context);
        }
    }
}