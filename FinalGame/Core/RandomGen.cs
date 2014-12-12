using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FinalGame
{
    public static class RandomGen
    {
        static Random random = new Random();

        public static int Next(int max)
        {
            return random.Next(max);
        }

        public static int Next()
        {
            return random.Next();
        }

        public static float NextFloat()
        {
            return (float)random.NextDouble();
        }

        public static Color RandomColor()
        {
            byte r = (byte)Next(255);

            byte g = (byte)Next(255);

            byte b = (byte)Next(255);

            return new Color(r, g, b);
        }

        public static Vector3 RandomVector3(int maxX, int maxY, int maxZ)
        {
            float x = ((2 * maxX) / 1) * (NextFloat()) - maxX;
            float y = ((2 * maxY) / 1) * (NextFloat()) - maxY;
            float z = ((2 * maxZ) / 1) * (NextFloat()) - maxZ;

            return new Vector3(x, y, z);
        }


        public static bool CalculateProbability(int probability)
        {
            if (probability <= 0)
                return false;
            else if (probability >= 100)
                return true;
            else
            {
                // Get a number between 1 and 100.
                int number = Next(99) + 1;

                // If the number is smaller than the probability percentage, return true.
                if (number <= probability)
                {
                    return true;
                }
                else return false;
            }
        }

        public static void MakeRandomClass(object cls, Dictionary<string,object> overrideFields = null)
        {
            Type type = cls.GetType();

            Type baseType = type.BaseType;

            FieldInfo[] baseFields = baseType.GetFields(BindingFlags.Instance |
                       BindingFlags.NonPublic |
                       BindingFlags.Public);

            FieldInfo[] clsfields = type.GetFields(BindingFlags.Instance |
                       BindingFlags.NonPublic |
                       BindingFlags.Public);

            List<FieldInfo> fieldsList = new List<FieldInfo>(baseFields.Concat<FieldInfo>(clsfields));

            FieldInfo[] fields = fieldsList.ToArray();

            foreach (FieldInfo field in fields)
            {
                if (overrideFields != null)
                {
                    if (overrideFields.ContainsKey(field.Name))
                    {
                        field.SetValue(cls, overrideFields[field.Name]);
                        continue;
                    }               
                }

                Type fieldType = field.FieldType;

                if (fieldType.Name == "Boolean")
                {
                    bool randomValue = CalculateProbability(50);
                    field.SetValue(cls, randomValue);
                }
                else if (fieldType.Name == "Vector3")
                {
                    Vector3 randomValue = RandomVector3(10, 10, 10);
                    field.SetValue(cls, randomValue);
                }
                else if (fieldType.Name == "Color")
                {
                    Color randomValue = RandomColor();
                    field.SetValue(cls, randomValue);
                }


            }
        }
    }
}
