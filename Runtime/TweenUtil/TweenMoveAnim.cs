#if PRIMETWEEN_INSTALLED && UNITASK_INSTALLED
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace CupkekGames.VFX
{
  public class TweenMoveAnim : TweenUtilBase
  {
    [SerializeField] private Vector3 moveTo;
    private Vector3 originalPosition;
    private void Awake()
    {
      originalPosition = transform.position;
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
      // reset scale
      transform.position = originalPosition;

      StartTween().Forget();
    }

    private async UniTask StartTween()
    {
      await UniTask.Delay((int)TweenDelay * 1000);

      base.TweenSequence = Sequence.Create(cycles: -1, TweenCycleMode)
        .Chain(Tween.Position(transform, endValue: originalPosition + moveTo, TweenDuration, TweenEase));
    }
  }
}
#endif
