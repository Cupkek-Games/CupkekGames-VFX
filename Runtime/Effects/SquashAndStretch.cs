using UnityEngine;
using PrimeTween;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.VFX
{
    public static partial class SquashAndStretch
    {
        [AutoStaticsCleanup]
        private static Dictionary<Transform, Sequence> _sequences = new Dictionary<Transform, Sequence>();

        public static void TakeDamage(Transform unitTransform, float bump, float duration)
        {
            Stop(unitTransform);

            // Squash: wider on X, compressed on Y.
            Vector3 squashScale = new Vector3(1 + bump, 1 - bump, 1f);
            // Stretch: compressed on X, taller on Y.
            Vector3 stretchScale = new Vector3(1 - bump, 1 + bump, 1f);

            // Create a sequence of tweens:
            Sequence sequence = Sequence.Create()
                .Chain(Tween.Scale(unitTransform, squashScale, duration, Ease.OutSine))
                .Chain(Tween.Scale(unitTransform, stretchScale, duration, Ease.OutSine))
                .Chain(Tween.Scale(unitTransform, Vector3.one, duration, Ease.OutSine))
                // A body destroyed mid-squash just drops the bookkeeping: nothing to warn about.
                .ChainCallback(() => _sequences.Remove(unitTransform), warnIfTargetDestroyed: false);

            _sequences.Add(unitTransform, sequence);
        }

        /// <summary>Ends a running squash on <paramref name="unitTransform"/> (call before the body is despawned or destroyed).</summary>
        public static void Stop(Transform unitTransform)
        {
            if (!_sequences.TryGetValue(unitTransform, out Sequence sequence)) return;
            sequence.Stop();
            _sequences.Remove(unitTransform);
        }
    }
}
