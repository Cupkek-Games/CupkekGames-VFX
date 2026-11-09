using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.VFX
{
    public abstract partial class ColorController : MonoBehaviour
    {
        protected Dictionary<Guid, ColorWeight> _overlays = new();
        protected Dictionary<Guid, CancellationTokenSource> _overlayRemoving = new();

        /// <summary>
        /// Weight of the original color when mixing with overlays.
        /// Set once at game init via <see cref="SetDefaultOriginalColorWeight"/>.
        /// </summary>
        [NoAutoStaticsCleanup]
        private static int _defaultOriginalColorWeight = 100;
        public static void SetDefaultOriginalColorWeight(int weight) => _defaultOriginalColorWeight = weight;
        protected int OriginalColorWeight => _defaultOriginalColorWeight;

        private void ApplyOverlay(Color? overlay, float weight)
        {
            if (overlay.HasValue)
            {
                LerpValue(overlay.Value, weight);
            }
            else
            {
                Revert();
            }
        }

        public abstract void Revert();
        public abstract void LerpValue(Color color, float weight);

        public void UpdateOverlay()
        {
            (Color? mix, float weight) = GetOverlayMix();

            ApplyOverlay(mix, weight);
        }

        public (Color? mixedColor, float totalWeight) GetOverlayMix()
        {
            if (_overlays.Count == 0)
            {
                return (null, 0);
            }

            return ColorUtils.MixColorsWithWeights(_overlays.Values);
        }

        public Guid AddColor(Color color, int weight)
        {
            Guid id = Guid.NewGuid();
            _overlays.Add(id, new ColorWeight(color, weight));

            UpdateOverlay();

            return id;
        }

        public void RemoveColor(Guid id)
        {
            if (_overlayRemoving.Remove(id, out CancellationTokenSource cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            _overlays.Remove(id);
            UpdateOverlay();
        }


        public async UniTaskVoid AddColor(Color color, int weight, int delay)
        {
            Guid id = AddColor(color, weight);

            if (delay <= 0)
            {
                return;
            }

            CancellationTokenSource cts = new CancellationTokenSource();

            _overlayRemoving.Add(id, cts);

            await UniTask.Delay(delay, cancellationToken: cts.Token);

            RemoveColor(id);
        }

        /// <summary>
        /// <paramref name="color"/> laid on at <paramref name="weight"/>, held <paramref name="holdMs"/>, then eased out
        /// to nothing over <paramref name="fadeMs"/> (its weight stepping down each frame) instead of dropping at once.
        /// A fade of 0 is <see cref="AddColor(Color, int, int)"/>.
        /// </summary>
        public async UniTaskVoid AddColorFading(Color color, int weight, int holdMs, int fadeMs)
        {
            if (fadeMs <= 0)
            {
                AddColor(color, weight, holdMs).Forget();
                return;
            }

            Guid id = AddColor(color, weight);
            CancellationTokenSource cts = new CancellationTokenSource();
            _overlayRemoving.Add(id, cts);
            try
            {
                if (holdMs > 0) await UniTask.Delay(holdMs, cancellationToken: cts.Token);

                float passed = 0f;
                float seconds = fadeMs / 1000f;
                while (passed < seconds)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                    passed += Time.deltaTime;
                    float left = 1f - Mathf.Clamp01(passed / seconds);
                    // Eased out: quick at first, then lingering faintly.
                    int now = Mathf.RoundToInt(weight * left * left);
                    if (now <= 0) break;
                    if (!_overlays.ContainsKey(id)) return;
                    _overlays[id] = new ColorWeight(color, now);
                    UpdateOverlay();
                }
            }
            catch (OperationCanceledException)
            {
                // Removed by RemoveColor (which already took it off).
                return;
            }

            if (this != null) RemoveColor(id);
        }
    }
}


