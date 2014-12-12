using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalGame
{
    public static class Helpers
    {

        #region Gaussian blur.

        // Store last calculated blur values for quick access.
        private static float[] lastSampleWeights;
        private static Vector2[] lastSampleOffsets;

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        public static void SetBlurEffectParameters(Effect blurEffect, float dx, float dy)
        {
            float[] sampleWeights;
            Vector2[] sampleOffsets;

            if (lastSampleWeights == null && lastSampleOffsets == null)
            {
                EffectParameter weightsParameter = blurEffect.Parameters["SampleWeights"];

                // Look up how many samples our gaussian blur effect supports.
                int sampleCount = weightsParameter.Elements.Count;

                // Create temporary arrays for computing our filter settings.
                sampleWeights = new float[sampleCount];
                sampleOffsets = new Vector2[sampleCount];

                // The first sample always has a zero offset.
                sampleWeights[0] = ComputeGaussian(0);
                sampleOffsets[0] = new Vector2(0);

                // Maintain a sum of all the weighting values.
                float totalWeights = sampleWeights[0];

                // Add pairs of additional sample taps, positioned
                // along a line in both directions from the center.
                for (int i = 0; i < sampleCount / 2; i++)
                {
                    // Store weights for the positive and negative taps.
                    float weight = ComputeGaussian(i + 1);

                    sampleWeights[i * 2 + 1] = weight;
                    sampleWeights[i * 2 + 2] = weight;

                    totalWeights += weight * 2;

                    // To get the maximum amount of blurring from a limited number of
                    // pixel shader samples, we take advantage of the bilinear filtering
                    // hardware inside the texture fetch unit. If we position our texture
                    // coordinates exactly halfway between two texels, the filtering unit
                    // will average them for us, giving two samples for the price of one.
                    // This allows us to step in units of two texels per sample, rather
                    // than just one at a time. The 1.5 offset kicks things off by
                    // positioning us nicely in between two texels.
                    float sampleOffset = i * 2 + 1.5f;

                    Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                    // Store texture coordinate offsets for the positive and negative taps.
                    sampleOffsets[i * 2 + 1] = delta;
                    sampleOffsets[i * 2 + 2] = -delta;
                }

                // Normalize the list of sample weightings, so they will always sum to one.
                for (int i = 0; i < sampleWeights.Length; i++)
                {
                    sampleWeights[i] /= totalWeights;
                }

                lastSampleWeights = sampleWeights;
                lastSampleOffsets = sampleOffsets;

            }
            else
            {
                sampleWeights = lastSampleWeights;
                sampleOffsets = lastSampleOffsets;
            }

            // Tell the effect about our new filter settings.
            blurEffect.Parameters["SampleWeights"].SetValue(sampleWeights);
            blurEffect.Parameters["SampleOffsets"].SetValue(sampleOffsets);

        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        public static float ComputeGaussian(float n)
        {
            //  float theta = Settings.BlurAmount;
            float theta = 2;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        #endregion

    }
}
