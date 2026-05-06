using UnityEngine;
using PrimeTween;
using System.Collections.Generic;

namespace CupkekGames.VFX
{
    public static class SquashAndStretch
    {
        private static Dictionary<Transform, Sequence> _sequences = new Dictionary<Transform, Sequence>();

        public static void TakeDamage(Transform unitTransform, float bump, float duration)
        {
            if (_sequences.ContainsKey(unitTransform))
            {
                _sequences[unitTransform].Stop();
                _sequences.Remove(unitTransform);
            }

            // Squash: wider on X, compressed on Y.
            Vector3 squashScale = new Vector3(1 + bump, 1 - bump, 1f);
            // Stretch: compressed on X, taller on Y.
            Vector3 stretchScale = new Vector3(1 - bump, 1 + bump, 1f);

            // Create a sequence of tweens:
            Sequence sequence = Sequence.Create()
                .Chain(Tween.Scale(unitTransform, squashScale, duration, Ease.OutSine))
                .Chain(Tween.Scale(unitTransform, stretchScale, duration, Ease.OutSine))
                .Chain(Tween.Scale(unitTransform, Vector3.one, duration, Ease.OutSine))
                .ChainCallback(() => _sequences.Remove(unitTransform));

            _sequences.Add(unitTransform, sequence);
        }
    }
}
