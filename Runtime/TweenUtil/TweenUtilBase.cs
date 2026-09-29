using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace CupkekGames.VFX
{
  public abstract class TweenUtilBase : MonoBehaviour
  {
    public float TweenDuration = 0.5f;
    public float TweenDelay = 0f;
    public Sequence.SequenceCycleMode TweenCycleMode = Sequence.SequenceCycleMode.Rewind;
    public Ease TweenEase = Ease.InOutSine;
    [System.NonSerialized] public Sequence? TweenSequence = null;

    private void OnDisable()
    {
      TweenSequence?.Stop();
    }

    /// <summary>
    /// Waits <see cref="TweenDelay"/>. False when the component was destroyed
    /// or disabled meanwhile (a scene unloading mid-delay): the caller must
    /// not touch its transform or start a tween then.
    /// </summary>
    protected async UniTask<bool> WaitStartDelay()
    {
      bool cancelled = await UniTask
        .Delay((int)(TweenDelay * 1000), cancellationToken: destroyCancellationToken)
        .SuppressCancellationThrow();
      return !cancelled && isActiveAndEnabled;
    }
  }
}
