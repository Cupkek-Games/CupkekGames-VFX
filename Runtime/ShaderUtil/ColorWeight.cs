using UnityEngine;

namespace CupkekGames.VFX
{
    public struct ColorWeight
    {
        public Color Color;
        public int Weight;
        public ColorWeight(Color color, int weight)
        {
            Color = color;
            Weight = weight;
            if (Weight < 0)
            {
                Weight = 0;
            }
        }
    }
}

