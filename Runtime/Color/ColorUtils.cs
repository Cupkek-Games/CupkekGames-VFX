using UnityEngine;
using System.Collections.Generic;
using System;

namespace CupkekGames.VFX
{
    public static class ColorUtils
    {
        public static Color MixColors(ICollection<Color> colors)
        {
            float r = 0f, g = 0f, b = 0f;

            foreach (var color in colors)
            {
                r += color.r;
                g += color.g;
                b += color.b;
            }

            // Average the components
            r /= colors.Count;
            g /= colors.Count;
            b /= colors.Count;

            return new Color(r, g, b);
        }

        public static (Color mixedColor, float totalWeight) MixColorsWithWeights(ICollection<ColorWeight> colors)
        {
            float r = 0f, g = 0f, b = 0f, totalWeight = 0f;

            foreach (ColorWeight colorWeight in colors)
            {
                Color color = colorWeight.Color;
                int weight = colorWeight.Weight;

                r += color.r * weight;
                g += color.g * weight;
                b += color.b * weight;
                totalWeight += weight;
            }

            // Average the components based on the total weight
            r /= totalWeight;
            g /= totalWeight;
            b /= totalWeight;

            return (new Color(r, g, b), totalWeight);
        }
        public static Color LerpColorsByWeights(Color colorA, float weightA, Color colorB, float weightB)
        {
            float totalWeight = weightA + weightB;

            return Color.Lerp(colorA, colorB, weightB / totalWeight);
        }
    }
}