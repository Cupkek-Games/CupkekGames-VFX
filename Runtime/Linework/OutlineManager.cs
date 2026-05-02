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

        public void RemoveOutline(GameObject parent, int outlineIndex)
        {
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

        public abstract void SetSharedWidth(float value);
        public abstract void SetWidth(float value, int outlineIndex);
        public abstract void SetColor(Color color, int outlineIndex);
    }
}