#if PRIMETWEEN_INSTALLED && UNITASK_INSTALLED
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

namespace CupkekGames.VFX
{
  public abstract class TweenUtilBase : MonoBehaviour
  {
    public float TweenDuration = 0.5f;
    public float TweenDelay = 0f;
    public CycleMode TweenCycleMode = CycleMode.Rewind;
    public Ease TweenEase = Ease.InOutSine;
    public Sequence? TweenSequence = null;

    private void OnDisable()
    {
      TweenSequence?.Stop();
    }
  }
}
#endif
