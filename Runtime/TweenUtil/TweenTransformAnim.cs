using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace CupkekGames.VFX
{
  public class TweenTransformAnim : TweenUtilBase
  {
    [SerializeField] private Vector3 _addPosition;
    [SerializeField] private Vector3 _addRotation;
    [SerializeField] private float _mulScale;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private void Awake()
    {
      originalPosition = transform.position;
      originalScale = transform.localScale;
      originalRotation = transform.rotation;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
      // reset scale
      transform.position = originalPosition;
      transform.localScale = originalScale;
      transform.rotation = originalRotation;
      StartTween().Forget();
    }

    private async UniTask StartTween()
    {
      await UniTask.Delay((int)TweenDelay * 1000);

      base.TweenSequence = Sequence.Create(cycles: -1, TweenCycleMode)
        .Group(Tween.Position(transform, endValue: originalPosition + _addPosition, TweenDuration, TweenEase))
        .Group(Tween.Rotation(transform, endValue: originalRotation * Quaternion.Euler(_addRotation), TweenDuration, TweenEase))
        .Group(Tween.Scale(transform, endValue: originalScale * _mulScale, TweenDuration, TweenEase));
    }
  }
}