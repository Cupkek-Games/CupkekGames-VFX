using UnityEngine;
using PrimeTween;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.VFX
{
    public static partial class SquashAndStretch
    {
        /// <summary>A running squash and the scale it returns to.</summary>
        private readonly struct Running
        {
            public readonly Sequence Sequence;
            public readonly Vector3 Rest;

            public Running(Sequence sequence, Vector3 rest)
            {
                Sequence = sequence;
                Rest = rest;
            }
        }

        [AutoStaticsCleanup]
        private static Dictionary<Transform, Running> _sequences = new Dictionary<Transform, Running>();

        /// <summary>Squash, stretch and back, each step <paramref name="duration"/> seconds.</summary>
        public static void TakeDamage(Transform unitTransform, float bump, float duration)
        {
            TakeDamage(unitTransform, bump, duration, duration, duration);
        }

        /// <summary>
        /// A hit's squash: wider and lower over <paramref name="squashSeconds"/>, then narrower and taller over
        /// <paramref name="stretchSeconds"/>, then settling back over <paramref name="settleSeconds"/>. Scaled from the
        /// transform's own resting scale (a model imported at another scale keeps it), so a hit during a hit starts
        /// from the same rest.
        /// </summary>
        public static void TakeDamage(Transform unitTransform, float bump, float squashSeconds, float stretchSeconds,
            float settleSeconds)
        {
            Vector3 rest = _sequences.TryGetValue(unitTransform, out Running running) ? running.Rest : unitTransform.localScale;
            Stop(unitTransform);

            // Squash: wider on X, compressed on Y. Stretch: compressed on X, taller on Y.
            Vector3 squashScale = Vector3.Scale(rest, new Vector3(1 + bump, 1 - bump, 1f));
            Vector3 stretchScale = Vector3.Scale(rest, new Vector3(1 - bump * 0.6f, 1 + bump * 0.6f, 1f));

            Sequence sequence = Sequence.Create()
                .Chain(Tween.Scale(unitTransform, squashScale, squashSeconds, Ease.OutQuad))
                .Chain(Tween.Scale(unitTransform, stretchScale, stretchSeconds, Ease.InOutSine))
                .Chain(Tween.Scale(unitTransform, rest, settleSeconds, Ease.OutSine))
                // A body destroyed mid-squash just drops the bookkeeping: nothing to warn about.
                .ChainCallback(() => _sequences.Remove(unitTransform), warnIfTargetDestroyed: false);

            _sequences.Add(unitTransform, new Running(sequence, rest));
        }

        /// <summary>
        /// Ends a running squash on <paramref name="unitTransform"/>, back at its resting scale (call before the body
        /// is despawned or destroyed).
        /// </summary>
        public static void Stop(Transform unitTransform)
        {
            if (!_sequences.TryGetValue(unitTransform, out Running running)) return;
            running.Sequence.Stop();
            _sequences.Remove(unitTransform);
            if (unitTransform != null) unitTransform.localScale = running.Rest;
        }
    }
}
