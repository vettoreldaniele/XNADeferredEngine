using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalGame
{
        public enum QualityType
        {
            Low = 1,
            Medium,
            High,
        };

        public enum RenderingMode
        {
            Forward,
            Deferred,
        };

        public enum RenderingTechnique
        { 
            Realistic,
            Toon,
        };

        public enum DrawType
        {
            Edges = 1,
            Model,
            GBuffer,
            Scene,
            ShadowMap,
        };

        public enum ShadowBlurType
        {
            None = 0,          
            Gaussian15 = 1,
        }
}
