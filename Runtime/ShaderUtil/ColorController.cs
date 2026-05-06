using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;

namespace CupkekGames.VFX
{
    public abstract class ColorController : MonoBehaviour
    {
        protected Dictionary<Guid, ColorWeight> _overlays = new();
        protected Dictionary<Guid, CancellationTokenSource> _overlayRemoving = new();

        /// <summary>
        /// Weight of the original color when mixing with overlays.
        /// Set once at game init via <see cref="SetDefaultOriginalColorWeight"/>.
        /// </summary>
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
    }
}


