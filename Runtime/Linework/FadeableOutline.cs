using UnityEngine;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using CupkekGames.Fadeables;

namespace CupkekGames.VFX
{
  public class FadeableOutline : FadeableMono
  {
    private Renderer[] _renderers;
    private OutlineManager _controller;
    [SerializeField] private OutlineType _outlineType;
    [SerializeField] public int _outlineIndex;
    [SerializeField] public bool _removeOnOther = true;
    [SerializeField] private Color _colorStart = Color.white;
    [SerializeField] private Color _colorEnd = Color.white;

    protected override void Awake()
    {
      base.Awake();

      if (_outlineType == OutlineType.Wide)
      {
        _controller = ServiceLocator.Get<WideOutlineManager>();
      }
      else if (_outlineType == OutlineType.Soft)
      {
        _controller = ServiceLocator.Get<SoftOutlineManager>();
      }
      else
      {
        Debug.LogError($"[FadeableOutline] {gameObject.name}: _outlineType is not set! Current value: {_outlineType}. Set it to Wide or Soft in Unity.", this);
      }

      if (_controller == null)
      {
        Debug.LogError($"[FadeableOutline] {gameObject.name}: Failed to get OutlineManager! _outlineType={_outlineType}", this);
      }

      _renderers = GetComponentsInChildren<Renderer>();
    }

    protected override void OnEnable()
    {
      Fadeable.OnFadeInStart += OnFadeInStart;
      Fadeable.OnFadeOutStart += OnFadeOutStart;

      if (!_removeOnOther)
      {
        Fadeable.OnApply += OnLocalApply;
      }

      if (!_removeOnOther && _controller != null)
      {
        AddOutline();
      }

      base.OnEnable();
    }

    protected override void OnDisable()
    {
      Fadeable.OnFadeInStart -= OnFadeInStart;
      Fadeable.OnFadeOutStart -= OnFadeOutStart;

      if (!_removeOnOther)
      {
        Fadeable.OnApply -= OnLocalApply;
      }

      base.OnDisable();
    }

    /// <summary>
    /// When removeOnOther=false, the local Fadeable directly drives width and color.
    /// This handles SetFadedIn, SetFadedOut, FadeIn, FadeOut, and _onAwake/_onEnable actions.
    /// </summary>
    private void OnLocalApply()
    {
      if (_controller == null) return;

      _controller.SetWidth(Fadeable.Value, _outlineIndex);

      if (_colorStart != _colorEnd)
      {
        float t = Mathf.InverseLerp(Fadeable._out, Fadeable._in, Fadeable.Value);
        _controller.SetColor(Color.Lerp(_colorStart, _colorEnd, t), _outlineIndex);
      }
      else
      {
        _controller.SetColor(_colorStart, _outlineIndex);
      }
    }

    private void OnFadeInStart()
    {
      if (_controller == null)
      {
        Debug.LogError($"[FadeableOutline] {gameObject.name}: Cannot fade in - _controller is null! _outlineType={_outlineType}", this);
        return;
      }

      if (_removeOnOther)
      {
        if (_outlineIndex >= _controller.OutlineController.Count)
        {
          Debug.LogError($"[FadeableOutline] {gameObject.name}: Outline index {_outlineIndex} is out of range (controller has {_controller.OutlineController.Count} indices)", this);
          return;
        }

        ApplySettings();

        // With removeOnOther=true, we control add/remove per-object, but width is still shared per index
        AddOutline();
        _controller.OutlineController[_outlineIndex].Fadeable.FadeIn();
        _controller.OutlineController[_outlineIndex].Fadeable.OnFadeInStart += RemoveOutline;
        _controller.OutlineController[_outlineIndex].Fadeable.OnFadeOutComplete += RemoveOutline;
      }
      // removeOnOther=false: local Fadeable drives width via OnLocalApply, nothing extra needed
    }

    private void OnFadeOutStart()
    {
      if (_removeOnOther)
      {
        ApplySettings();

        AddOutline();
        _controller.OutlineController[_outlineIndex].Fadeable.FadeOut();
        _controller.OutlineController[_outlineIndex].Fadeable.OnFadeInStart += RemoveOutline;
        _controller.OutlineController[_outlineIndex].Fadeable.OnFadeOutComplete += RemoveOutline;
      }
      // removeOnOther=false: local Fadeable drives width via OnLocalApply, nothing extra needed
    }

    private void ApplySettings()
    {
      Fadeable fadeable = _controller.OutlineController[_outlineIndex].Fadeable;

      fadeable._fadeInDuration = Fadeable._fadeInDuration;
      fadeable._fadeOutDuration = Fadeable._fadeOutDuration;
      fadeable._fadeInDelay = Fadeable._fadeInDelay;
      fadeable._fadeOutDelay = Fadeable._fadeOutDelay;
      fadeable._in = Fadeable._in;
      fadeable._out = Fadeable._out;

      _controller.OutlineController[_outlineIndex].ColorStart = _colorStart;
      _controller.OutlineController[_outlineIndex].ColorEnd = _colorEnd;
    }

    public void SetOutlineIndex(int outlineIndex)
    {
      SetOutlineIndex(outlineIndex, resetWidth: true);
    }

    public void SetOutlineIndex(int outlineIndex, bool resetWidth)
    {
      if (_outlineIndex == outlineIndex)
      {
        return;
      }

      _controller.RemoveOutline(gameObject, _outlineIndex);
      _outlineIndex = outlineIndex;
      _controller.AddOutline(gameObject, _outlineIndex);

      // When resetWidth=true, snap the local fadeable to _out so the next FadeIn starts from there
      if (resetWidth && !_removeOnOther)
      {
        Fadeable.SetFadedOut(); // triggers OnApply -> OnLocalApply -> SetWidth(_out)
      }
    }

    public void AddOutline()
    {
      if (_controller == null)
      {
        Debug.LogError($"[FadeableOutline] {gameObject.name}: Cannot AddOutline - _controller is null! _outlineType={_outlineType}", this);
        return;
      }

      if (_renderers == null || _renderers.Length == 0)
      {
        Debug.LogWarning($"[FadeableOutline] {gameObject.name}: No renderers found! Cannot add outline.", this);
        return;
      }

      _controller.AddOutline(gameObject, _renderers, _outlineIndex);
    }

    public void RemoveOutline()
    {
      _controller.OutlineController[_outlineIndex].Fadeable.OnFadeInStart -= RemoveOutline;
      _controller.OutlineController[_outlineIndex].Fadeable.OnFadeOutComplete -= RemoveOutline;
      _controller.RemoveOutline(gameObject, _outlineIndex);
    }

    #region Temporary Index Swap

    private int _baseOutlineIndex = -1;
    private System.Action _onRestoreComplete;

    /// <summary>
    /// Temporarily swaps to a different outline index and fades in.
    /// Call FadeOutAndRestoreIndex() to reverse this.
    /// </summary>
    public void SetIndexAndFadeIn(int targetIndex)
    {
      if (_controller == null) return;

      // Cancel any other active index swap on this controller
      _controller.SetActiveIndexSwap(this);

      // Store base index to restore later
      _baseOutlineIndex = _outlineIndex;

      // Swap to target index
      SetOutlineIndex(targetIndex);

      // Fade in the width
      Fadeable.FadeIn();
    }

    /// <summary>
    /// Fades out the outline width and restores the original index when complete.
    /// </summary>
    public void FadeOutAndRestoreIndex(System.Action onComplete = null)
    {
      if (_controller == null || _baseOutlineIndex < 0) return;

      _onRestoreComplete = onComplete;

      Fadeable.OnFadeOutComplete += OnFadeOutRestoreIndex;
      Fadeable.FadeOut();
    }

    /// <summary>
    /// Force-cancels any running fade and snaps back to the base index immediately.
    /// Called by OutlineManager when another outline takes over.
    /// </summary>
    public void ForceCompleteRestore()
    {
      Fadeable.Kill();
      Fadeable.OnFadeOutComplete -= OnFadeOutRestoreIndex;

      if (_baseOutlineIndex >= 0)
      {
        SetOutlineIndex(_baseOutlineIndex, resetWidth: false);
        _baseOutlineIndex = -1;
      }

      _controller.ClearActiveIndexSwap(this);

      _onRestoreComplete?.Invoke();
      _onRestoreComplete = null;
    }

    private void OnFadeOutRestoreIndex()
    {
      Fadeable.OnFadeOutComplete -= OnFadeOutRestoreIndex;

      if (_baseOutlineIndex >= 0)
      {
        SetOutlineIndex(_baseOutlineIndex, resetWidth: false);
        _baseOutlineIndex = -1;
      }

      _controller.ClearActiveIndexSwap(this);

      _onRestoreComplete?.Invoke();
      _onRestoreComplete = null;
    }

    #endregion
  }
}