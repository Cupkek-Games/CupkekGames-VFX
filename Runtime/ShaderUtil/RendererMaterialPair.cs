using UnityEngine;

namespace CupkekGames.VFX
{
    [System.Serializable]
    public class RendererMaterialPair
    {
        public Renderer Renderer;
        public int MaterialIndex;
        public Material GetMaterial()
        {
            return Renderer.materials[MaterialIndex];
        }
    }
}

