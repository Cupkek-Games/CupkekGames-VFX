using UnityEngine;
using System;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

namespace CupkekGames.VFX
{
  public class StartOutlineController : MonoBehaviour
  {
    private OutlineManager _controller;
    [SerializeField] private OutlineType _outlineType;
    [SerializeField] public float _width;
    [SerializeField] public Color _color = Color.white;

    [Header(("-1 for Shared"))] [SerializeField]
    public int _index = -1;

    private void Awake()
    {
      if (_outlineType == OutlineType.Wide)
      {
        _controller = ServiceLocator.Get<WideOutlineManager>();
      }
      else if (_outlineType == OutlineType.Soft)
      {
        _controller = ServiceLocator.Get<SoftOutlineManager>();
      }

      // Set width
      if (_index < 0)
      {
        _controller.SetSharedWidth(_width);
      }
      else
      {
        _controller.SetWidth(_width, _index);
      }

      // Set color (only if index is valid, color is not transparent/black)
      if (_index >= 0 && _color.a > 0f)
      {
        _controller.SetColor(_color, _index);
      }
    }
  }
}