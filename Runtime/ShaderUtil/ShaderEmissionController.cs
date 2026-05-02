using UnityEngine;

namespace CupkekGames.VFX
{
    public class ShaderEmissionController : ColorController
    {
        [SerializeField] private string _shaderPropertyName = "_EmissionColor";
        private Renderer[] _renderers;
        private Color[][] _originalColors;

        private void Awake()
        {
            _renderers = gameObject.GetComponentsInChildren<Renderer>();

            // Initialize the array to store original colors
            _originalColors = new Color[_renderers.Length][];
            // Store the original colors
            for (int i = 0; i < _renderers.Length; i++)
            {
                Material[] materials = _renderers[i].materials;
                _originalColors[i] = new Color[materials.Length];

                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j].HasProperty(_shaderPropertyName))
                    {
                        _originalColors[i][j] = materials[j].GetColor(_shaderPropertyName);
                    }
                    else
                    {
                        _originalColors[i][j] = Color.black;
                    }
                }
            }
        }

        private void ApplyOverlay(Color? overlay, float weight)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    return;
                }

                Material[] materials = _renderers[i].materials;

                for (int j = 0; j < materials.Length; j++)
                {
                    if (!materials[j].HasProperty(_shaderPropertyName))
                    {
                        continue;
                    }

                    if (overlay.HasValue)
                    {
                        Color lerpedColor = ColorUtils.LerpColorsByWeights(
                            _originalColors[i][j],
                            OriginalColorWeight,
                            overlay.Value,
                            weight);
                        materials[j].SetColor(_shaderPropertyName, lerpedColor);
                    }
                    else
                    {
                        materials[j].SetColor(_shaderPropertyName, _originalColors[i][j]);
                    }
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

