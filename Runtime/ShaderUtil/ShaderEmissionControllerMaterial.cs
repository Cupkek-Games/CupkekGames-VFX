using UnityEngine;

namespace CupkekGames.VFX
{
    public class ShaderEmissionControllerMaterial : ColorController
    {
        [SerializeField] private string _shaderPropertyName = "_EmissionColor";
        [SerializeField] private RendererMaterialPair[] _materials;
        private Color[] _originalColors;

        private void Awake()
        {
            // Initialize the array to store original colors
            _originalColors = new Color[_materials.Length];
            // Store the original colors
            for (int i = 0; i < _materials.Length; i++)
            {
                Material material = _materials[i].GetMaterial();
                if (material.HasProperty(_shaderPropertyName))
                {
                    _originalColors[i] = material.GetColor(_shaderPropertyName);
                }
                else
                {
                    _originalColors[i] = Color.black;
                }
            }
        }

        public Material GetMaterial(int index)
        {
            return _materials[index].GetMaterial();
        }

        private void ApplyOverlay(Color? overlay, float weight)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                Material material = GetMaterial(i);
                if (!material.HasProperty(_shaderPropertyName))
                {
                    continue;
                }

                if (overlay.HasValue)
                {
                    Color lerpedColor = ColorUtils.LerpColorsByWeights(
                        _originalColors[i],
                        OriginalColorWeight,
                        overlay.Value,
                        weight);
                    material.SetColor(_shaderPropertyName, lerpedColor);
                }
                else
                {
                    material.SetColor(_shaderPropertyName, _originalColors[i]);
                }
            }
        }

        public override void Revert()
        {
            ApplyOverlay(null, 0);
        }

        public override void LerpValue(Color color, float weight)
        {
            ApplyOverlay(color, weight);
        }
    }
}

