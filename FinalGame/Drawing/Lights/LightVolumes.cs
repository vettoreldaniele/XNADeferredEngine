using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalGame
{
    static class LightVolumes
    {
        public static ContentManager manager;

        public static Model pointLightVolume;
        public static Model spotLightVolume;

        public static void LoadLightVolumes()
        {
            pointLightVolume = manager.Load<Model>("Models/PointLightVolume");
            spotLightVolume = manager.Load<Model>("Models/SpotLightVolume");
        }

    }
}
