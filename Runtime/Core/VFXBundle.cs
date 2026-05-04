using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Threading;
using CupkekGames.TimeSystem;
using System.Collections.Generic;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using CupkekGames.Transforms;
using CupkekGames.Audio;
using CupkekGames.Pool;

namespace CupkekGames.VFX
{
  [Serializable]
  public class VFXBundle
  {
    [SerializeField] private GameObject _vfxPrefab;
    [SerializeField] private SFXPlayerSO _sfxPlayer;
    [Header("Optional")][SerializeField] private Transform _transform;
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private Quaternion _rotation = Quaternion.identity;
    [SerializeField] private Vector3 _scale = Vector3.one;

    [SerializeField] private int _destroyDelayMS = 1000;

    public int DestroyDelayMS
    {
      get => _destroyDelayMS;
      set => _destroyDelayMS = value;
    }

    // Tweening
    [SerializeField] private Vector3 _tweenPosition = Vector3.zero;
    [SerializeField] private float _tweenDuration = 0.5f;
    [SerializeField] private Ease _tweenEase = Ease.OutSine;

    // State
    private GameObjectPoolList _poolList = null;
    private Guid _id = Guid.Empty;

    public void Prewarm(GameObject parent, int defaultCapacity = 1, int maxSize = 2)
    {
      if (_vfxPrefab == null)
      {
        return;
      }

      if (_poolList != null)
      {
        Debug.LogWarning("VFXBundle pool already initialized");
        return;
      }

      if (parent.GetComponent<GameObjectPoolList>() == null)
      {
        // Add the script to the targetGameObject
        parent.AddComponent(typeof(GameObjectPoolList));
      }

      _poolList = parent.GetComponent<GameObjectPoolList>();
      _id = _poolList.CreateNewPool(_vfxPrefab, defaultCapacity, maxSize, false);

      GameObjectPool pool = _poolList.GetPool(_id);
      pool.OnCreateEvent += OnCreateEvent;
      pool.OnDestroyObjectEvent += OnDestroyEvent;
      pool.Prewarm();

      // Debug.Log("Prewarmed VFXBundle: " + _vfxPrefab.name + " with id: " + _id);
    }

    private void OnCreateEvent(GameObject gameObject)
    {
      RenderFeatureManager manager = ServiceLocator.Get<RenderFeatureManager>();
      manager.Register(gameObject, true);
    }

    private void OnDestroyEvent(GameObject gameObject)
    {
      RenderFeatureManager manager = ServiceLocator.Get<RenderFeatureManager>(true);
      if (manager == null)
      {
        return;
      }

      manager.Unregister(gameObject, true);
    }

    public void Dispose()
    {
      if (_poolList != null)
      {
        GameObjectPool pool = _poolList.GetPool(_id);
        if (pool != null)
        {
          pool.OnCreateEvent -= OnCreateEvent;
          pool.OnDestroyObjectEvent -= OnDestroyEvent;
        }
        _poolList.Dispose();
        _poolList = null;
      }
    }

    public async UniTask<GameObject> Play(GameObject parent, Transform transform, CancellationToken? ct,
      TimeBundle timeBundle,
      RenderFeatureManager renderFeatureManager)
    {
      return await Play(parent, transform.position, transform.rotation, ct, timeBundle, renderFeatureManager);
    }

    public async UniTask<GameObject> Play(
      GameObject parent,
      Vector3 position,
      Quaternion rotation,
      CancellationToken? ct,
      TimeBundle timeBundle,
      RenderFeatureManager renderFeatureManager,
      bool persistent = false)
    {
      if (ct is { IsCancellationRequested: true })
      {
        return null;
      }

      if (timeBundle == null)
      {
        throw new Exception("TimeBundle is null");
      }

      GameObject vfx = null;

      Vector3 vfxPosition = position + rotation * _offset;
      Quaternion vfxRotation = rotation * _rotation;

      // Play VFX
      if (_vfxPrefab != null)
      {
        vfx = _poolList.GetPool(_id).Pool.Get();
        vfx.transform.SetPositionAndRotation(vfxPosition, vfxRotation);
        // Set scale
        vfx.transform.localScale = _scale;
        TransformUtils.SetScaleRecursive(vfx.transform, _scale);

        vfx.SetActive(true);

        renderFeatureManager?.UnDarkenAsync(vfx, true).Forget();

        // Debug.Log("Played VFX: " + vfx.name + " with id: " + _id);

        // Register TimeScale
        timeBundle.TimeScaleParticleSystem.Add(vfx);
        timeBundle.TimeScaleTrailRenderer.Add(vfx);
        timeBundle.TimeScaleVisualEffect.Add(vfx);
      }

      // Play SFX
      if (_sfxPlayer != null)
      {
        Transform sfxTransform = vfx != null ? vfx.transform : parent.transform;
        _sfxPlayer.Play(sfxTransform);
        _sfxPlayer.RegisterTimeScale(timeBundle, sfxTransform);
      }

      if (persistent)
      {
        return vfx;
      }

      if (_tweenPosition != Vector3.zero)
      {
        Vector3 dashWorldSpaceDisplacement = vfxRotation * _tweenPosition;

        Vector3 endPosition = vfxPosition + dashWorldSpaceDisplacement;

        try
        {
          Tween tween = Tween.Position(vfx.transform, endPosition, _tweenDuration, ease: _tweenEase);

          if (timeBundle != null)
          {
            timeBundle.TimeScaleTween.Add(tween);
          }

          await tween.ToYieldInstruction().ToUniTask(cancellationToken: ct ?? CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
          // Handle cancellation if needed
          if (vfx != null && vfx.activeSelf)
          {
            vfx.SetActive(false);
          }

          return vfx;
        }
      }

      try
      {
        // Pass cancellation token to properly handle cancellation during delay
        await timeBundle.TimeContext.DelayAsync(_destroyDelayMS / 1000f, ct ?? CancellationToken.None);
      }
      catch (OperationCanceledException)
      {
        // Handle cancellation during delay - deactivate GameObject before returning
        if (vfx != null && vfx.activeSelf)
        {
          vfx.SetActive(false);
        }

        return vfx;
      }

      if (vfx != null && vfx.activeSelf)
      {
        vfx.SetActive(false);
      }

      timeBundle.ClearInactive();

      return vfx;
    }
  }
}