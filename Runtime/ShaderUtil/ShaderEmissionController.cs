using UnityEngine;

namespace CupkekGames.VFX
{
    public class ShaderEmissionController : ColorController
    {
        [SerializeField] private string _shaderPropertyName = "_EmissionColor";
        [Tooltip("The keyword that switches a material's emission on: switched on while an overlay shows on a material that has it off (a flash on a body that doesn't glow), and off again after. Empty: never touched.")]
        [SerializeField] private string _emissionKeyword = "_EMISSION";
        private Renderer[] _renderers;
        private Color[][] _originalColors;
        // Whether each material's emission was off to begin with.
        private bool[][] _emissionWasOff;

        private void Awake()
        {
            _renderers = gameObject.GetComponentsInChildren<Renderer>();

            // Initialize the array to store original colors
            _originalColors = new Color[_renderers.Length][];
            _emissionWasOff = new bool[_renderers.Length][];
            // Store the original colors
            for (int i = 0; i < _renderers.Length; i++)
            {
                Material[] materials = _renderers[i].materials;
                _originalColors[i] = new Color[materials.Length];
                _emissionWasOff[i] = new bool[materials.Length];

                for (int j = 0; j < materials.Length; j++)
                {
                    _emissionWasOff[i][j] = !string.IsNullOrEmpty(_emissionKeyword)
                                            && materials[j].HasProperty(_shaderPropertyName)
                                            && !materials[j].IsKeywordEnabled(_emissionKeyword);
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
                        if (_emissionWasOff[i][j]) materials[j].EnableKeyword(_emissionKeyword);
                        materials[j].SetColor(_shaderPropertyName, lerpedColor);
                    }
                    else
                    {
                        materials[j].SetColor(_shaderPropertyName, _originalColors[i][j]);
                        if (_emissionWasOff[i][j]) materials[j].DisableKeyword(_emissionKeyword);
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

