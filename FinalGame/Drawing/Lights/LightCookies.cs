using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace FinalGame
{
    static class LightCookies
    {
        public static ContentManager manager;

        public static Texture2D spotRadialCookie;

        public static void LoadCookies()
        {
            spotRadialCookie = manager.Load<Texture2D>("Textures/SpotRadialCookie");
        }
    }
}
