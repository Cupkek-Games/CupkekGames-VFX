using UnityEngine;


namespace CupkekGames.VFX
{
  public class FogSettingsOnLoad : MonoBehaviour
  {
    [SerializeField] private UnityEngine.Rendering.Universal.FullScreenPassRendererFeature _fullScreenPassRendererFeature;
    [SerializeField] private Vector2 _colorStartEnd;
    [SerializeField] private Vector2 _fadeStartEnd;

    private Material _material;

    private void Awake()
    {
      _material = _fullScreenPassRendererFeature.passMaterial;

      Apply();
    }

    public void Apply()
    {
      if (_material == null)
      {
        _material = _fullScreenPassRendererFeature.passMaterial;
      }

      _material.SetVector("_Color_Start_End", _colorStartEnd);
      _material.SetVector("_Fade_Start_End", _fadeStartEnd);
    }
  }
}