#if PRIMETWEEN_INSTALLED && UNITASK_INSTALLED
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace CupkekGames.VFX
{
  public class TweenScaleAnim : TweenUtilBase
  {
    [SerializeField] private float scaleUp = 1.2f;
    private Vector3 originalScale;
    private void Awake()
    {
      originalScale = transform.localScale;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
      // reset scale
      transform.localScale = originalScale;
      StartTween().Forget();
    }

    private async UniTask StartTween()
    {
      await UniTask.Delay((int)TweenDelay * 1000);

      base.TweenSequence = Sequence.Create(cycles: -1, TweenCycleMode)
        .Chain(Tween.Scale(transform, endValue: originalScale * scaleUp, TweenDuration, TweenEase));
    }
  }
}
#endif
