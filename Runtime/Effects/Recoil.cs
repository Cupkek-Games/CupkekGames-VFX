using UnityEngine;
using PrimeTween;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.VFX
{
    /// <summary>
    /// A hit's nudge: a body pushed a little along a direction and springing back to where it rests. Moves the
    /// transform's local position only, so put it on a body that nothing else places (a model under the object that
    /// walks), and an animation playing on it is left alone.
    /// </summary>
    public static partial class Recoil
    {
        /// <summary>A running nudge and the local position it returns to.</summary>
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
        private static Dictionary<Transform, Running> _running = new Dictionary<Transform, Running>();

        /// <summary>
        /// Pushes <paramref name="body"/> by <paramref name="worldOffset"/> (world space) over
        /// <paramref name="outSeconds"/>, then back to rest over <paramref name="backSeconds"/>. A nudge during a nudge
        /// starts again from the same rest.
        /// </summary>
        public static void Push(Transform body, Vector3 worldOffset, float outSeconds, float backSeconds)
        {
            if (body == null) return;
            Vector3 rest = _running.TryGetValue(body, out Running running) ? running.Rest : body.localPosition;
            Stop(body);

            Transform parent = body.parent;
            Vector3 local = parent != null ? parent.InverseTransformVector(worldOffset) : worldOffset;
            Sequence sequence = Sequence.Create()
                .Chain(Tween.LocalPosition(body, rest + local, outSeconds, Ease.OutQuad))
                .Chain(Tween.LocalPosition(body, rest, backSeconds, Ease.OutSine))
                .ChainCallback(() => _running.Remove(body), warnIfTargetDestroyed: false);

            _running.Add(body, new Running(sequence, rest));
        }

        /// <summary>Ends a running nudge, back at rest (call before the body is despawned or destroyed).</summary>
        public static void Stop(Transform body)
        {
            if (body == null || !_running.TryGetValue(body, out Running running)) return;
            running.Sequence.Stop();
            _running.Remove(body);
            body.localPosition = running.Rest;
        }
    }
}
