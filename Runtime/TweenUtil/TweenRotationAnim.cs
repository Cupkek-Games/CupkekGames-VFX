#if PRIMETWEEN_INSTALLED && UNITASK_INSTALLED
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace CupkekGames.VFX
{
  public class TweenRotationAnim : TweenUtilBase
  {
    [SerializeField] private Vector3 rotation;
    private Quaternion originalRotation;
    private void Awake()
    {
      originalRotation = transform.rotation;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
      transform.rotation = originalRotation;
      StartTween().Forget();
    }

    private async UniTask StartTween()
    {
      await UniTask.Delay((int)TweenDelay * 1000);

      base.TweenSequence = Sequence.Create(cycles: -1, TweenCycleMode)
        .Chain(Tween.Rotation(transform, endValue: originalRotation * Quaternion.Euler(rotation), TweenDuration, TweenEase));
    }
  }
}
#endif
