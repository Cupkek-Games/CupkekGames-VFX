using UnityEngine;
using System.Collections.Generic;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

namespace CupkekGames.VFX
{
    public abstract class OutlineManager : ServiceProvider
    {
        [SerializeField] private RenderingLayerMask[] _outlineLayer;
        private List<OutlineController> _outlineController = new();
        public List<OutlineController> OutlineController => _outlineController;
        private List<Dictionary<GameObject, Renderer[]>> _renderers = new();
        private List<Dictionary<Renderer, uint>> _originalLayers = new();
        // Who holds each object's outline, per index (Hold / Release).
        private List<Dictionary<GameObject, HashSet<object>>> _holders = new();

        /// <summary>
        /// The FadeableOutline currently performing a temporary index swap on this manager.
        /// Only one can be active at a time to prevent width conflicts on the same outline index.
        /// </summary>
        private FadeableOutline _activeIndexSwap;

        public void SetActiveIndexSwap(FadeableOutline fade)
        {
            if (_activeIndexSwap != null && _activeIndexSwap != fade)
            {
                _activeIndexSwap.ForceCompleteRestore();
            }
            _activeIndexSwap = fade;
        }

        public void ClearActiveIndexSwap(FadeableOutline fade)
        {
            if (_activeIndexSwap == fade)
            {
                _activeIndexSwap = null;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < _outlineLayer.Length; i++)
            {
                _renderers.Add(new());
                _originalLayers.Add(new());
                _holders.Add(new());

                _outlineController.Add(new(this, i));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var fadeable in _outlineController)
            {
                fadeable.Kill();
            }
        }

        private void OnEnable()
        {
            foreach (var fadeable in _outlineController)
            {
                fadeable.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (var fadeable in _outlineController)
            {
                fadeable.OnDisable();
            }
        }

        public void AddOutline(GameObject parent, int outlineIndex)
        {
            if (_renderers[outlineIndex].ContainsKey(parent))
            {
                return;
            }

            Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();

            if (renderers != null && renderers.Length > 0)
            {
                AddOutline(parent, renderers, outlineIndex);
            }
        }

        public void AddOutline(GameObject parent, Renderer[] renderers, int outlineIndex)
        {
            if (_renderers[outlineIndex].ContainsKey(parent))
            {
                return;
            }

            _renderers[outlineIndex].Add(parent, renderers);

            foreach (var renderer in renderers)
            {
                _originalLayers[outlineIndex].Add(renderer, renderer.renderingLayerMask);
                renderer.renderingLayerMask |= _outlineLayer[outlineIndex];
            }

            OutlineReference reference = parent.GetComponent<OutlineReference>();
            if (reference == null)
            {
                reference = parent.AddComponent<OutlineReference>();
            }

            reference.Add(this, outlineIndex);
        }

        /// <summary>
        /// <paramref name="source"/> holds the outline of index <paramref name="outlineIndex"/>
        /// on <paramref name="parent"/>. Several sources can hold the same one (a hover, a
        /// selection, a marker): it is drawn from the first hold and stays until the last
        /// source lets go (<see cref="Release"/>), so one source never takes away another's.
        /// It draws round the parent's meshes as they are at the first hold; particles are
        /// left unlined. <see cref="RemoveOutline"/> ends it for every holder.
        /// </summary>
        public void Hold(GameObject parent, object source, int outlineIndex)
        {
            if (parent == null) throw new System.ArgumentNullException(nameof(parent));
            if (source == null) throw new System.ArgumentNullException(nameof(source));

            if (!_holders[outlineIndex].TryGetValue(parent, out HashSet<object> holders))
            {
                holders = new HashSet<object>();
                _holders[outlineIndex].Add(parent, holders);
            }

            if (!holders.Add(source) || holders.Count > 1) return;
            AddOutline(parent, Meshes(parent), outlineIndex);
        }

        /// <summary>
        /// <paramref name="source"/> lets go of the outline it held on <paramref name="parent"/>;
        /// the outline goes when no source holds it. A parent already destroyed has nothing to release.
        /// </summary>
        public void Release(GameObject parent, object source, int outlineIndex)
        {
            if (parent == null) return;
            if (!_holders[outlineIndex].TryGetValue(parent, out HashSet<object> holders)) return;
            if (!holders.Remove(source) || holders.Count > 0) return;

            RemoveOutline(parent, outlineIndex);
        }

        /// <summary>Whether any source holds the outline of index <paramref name="outlineIndex"/> on <paramref name="parent"/>.</summary>
        public bool IsHeld(GameObject parent, int outlineIndex)
        {
            return parent != null
                && _holders[outlineIndex].TryGetValue(parent, out HashSet<object> holders)
                && holders.Count > 0;
        }

        private static Renderer[] Meshes(GameObject parent)
        {
            List<Renderer> meshes = new();
            foreach (Renderer renderer in parent.GetComponentsInChildren<Renderer>())
            {
                if (renderer is ParticleSystemRenderer) continue;
                meshes.Add(renderer);
            }

            return meshes.ToArray();
        }

        public void RemoveOutline(GameObject parent, int outlineIndex)
        {
            _holders[outlineIndex].Remove(parent);
            if (_renderers[outlineIndex].Remove(parent, out Renderer[] renderers))
            {
                foreach (var renderer in renderers)
                {
                    if (_originalLayers[outlineIndex].Remove(renderer, out uint original))
                    {
                        renderer.renderingLayerMask &= ~(uint)_outlineLayer[outlineIndex];
                    }
                }
            }
        }

        public void RemoveSilent(GameObject parent, int outlineIndex)
        {
            if (_renderers[outlineIndex].Remove(parent, out Renderer[] renderers))
            {
                foreach (var renderer in renderers)
                {
                    _originalLayers[outlineIndex].Remove(renderer);
                }
            }
        }

        /// <summary>
        /// False when the implementation only supports a shared width (e.g. SoftOutline's
        /// kernel size) — per-index width calls are invalid there. Batch appliers like
        /// OutlineInitializer must check this before calling SetWidth.
        /// </summary>
        public virtual bool SupportsPerIndexWidth => true;

        public abstract void SetSharedWidth(float value);
        public abstract void SetWidth(float value, int outlineIndex);
        public abstract void SetColor(Color color, int outlineIndex);
    }
}