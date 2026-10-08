using System.Collections.Generic;
using CupkekGames.Services;
using PrimeTween;
using UnityEngine;

namespace CupkekGames.VFX
{
    /// <summary>
    /// Darkens the finished picture except a few spotlit renderers and every effect (an
    /// ultimate's caster and its VFX). Nothing in the scene is drawn again in another
    /// material, so every shader keeps its look: the spotlit renderers move to the spotlit
    /// layer, which the renderer marks in the stencil (<c>Hidden/CupkekGames/SpotlightMask</c>
    /// in two RenderObjects passes); <see cref="SpotlightDimFeature"/> darkens every unmarked
    /// pixel by the global <c>_CkgSpotlightDim</c>; effects sit on the effects layer, which the
    /// renderer leaves out of its own passes and draws after the dim. The dim fades on
    /// unscaled time.
    /// </summary>
    public class SceneSpotlight : ServiceProvider
    {
        /// <summary>The global the dim pass reads: 0 is off, 1 is black.</summary>
        public static readonly int DimId = Shader.PropertyToID("_CkgSpotlightDim");

        [Tooltip("The layer the spotlit renderers move to while the spotlight is on; the renderer marks it in the stencil.")]
        [SerializeField] private string _spotlitLayer = "Spotlit";

        [Tooltip("The layer every effect is put on; the renderer draws it after the dim.")]
        [SerializeField] private string _effectsLayer = "VFX";

        [Tooltip("How dark the rest of the picture gets: 0 leaves it, 1 is black.")]
        [SerializeField, Range(0f, 1f)] private float _strength = 0.8f;

        [Tooltip("Seconds the dim takes to come and to go, on unscaled time.")]
        [SerializeField, Min(0f)] private float _fadeSeconds = 0.2f;

        // The spotlit renderers' objects and the layers they had.
        private readonly Dictionary<GameObject, int> _spotlit = new Dictionary<GameObject, int>();
        private int _spotlitLayerIndex = -1;
        private int _effectsLayerIndex = -1;
        private Tween _fade;

        /// <summary>The spotlight is on (its dim may still be fading in).</summary>
        public bool IsOn { get; private set; }

        /// <summary>The dim right now, 0 to 1.</summary>
        public float Dim { get; private set; }

        /// <summary>The layer effects are put on.</summary>
        public int EffectsLayer => _effectsLayerIndex >= 0 ? _effectsLayerIndex : _effectsLayerIndex = RequireLayer(_effectsLayer);

        /// <summary>The layer spotlit renderers stand on while the spotlight is on.</summary>
        public int SpotlitLayer => _spotlitLayerIndex >= 0 ? _spotlitLayerIndex : _spotlitLayerIndex = RequireLayer(_spotlitLayer);

        protected override void Awake()
        {
            SetDim(0f);
            base.Awake();
        }

        protected override void OnDestroy()
        {
            _fade.Stop();
            Restore();
            SetDim(0f);
            base.OnDestroy();
        }

        public override void RegisterServices()
        {
            ServiceLocator.Register(this);
        }

        public override void UnregisterServices()
        {
            ServiceLocator.Remove(this);
        }

        /// <summary>
        /// Darkens everything but <paramref name="renderers"/> (renderer objects) and the effects.
        /// A spotlight already on moves to these renderers at once, without fading.
        /// </summary>
        public void Spotlight(ICollection<GameObject> renderers)
        {
            Restore();
            int layer = SpotlitLayer;
            foreach (GameObject renderer in renderers)
            {
                if (renderer == null || _spotlit.ContainsKey(renderer)) continue;
                _spotlit[renderer] = renderer.layer;
                renderer.layer = layer;
            }

            IsOn = true;
            FadeTo(_strength, null);
        }

        /// <summary>The dim fades out; the spotlit renderers go back to their layers once it has.</summary>
        public void ClearSpotlight()
        {
            if (!IsOn) return;
            IsOn = false;
            FadeTo(0f, Restore);
        }

        /// <summary>Puts <paramref name="root"/> and everything under it on the effects layer: it stays lit under the dim.</summary>
        public void MarkEffect(GameObject root)
        {
            if (root == null) return;
            int layer = EffectsLayer;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }

        /// <summary>Whether <paramref name="renderer"/> is spotlit right now.</summary>
        public bool IsSpotlit(GameObject renderer) => renderer != null && _spotlit.ContainsKey(renderer);

        private void FadeTo(float target, System.Action done)
        {
            _fade.Stop();
            if (_fadeSeconds <= 0f || Mathf.Approximately(Dim, target))
            {
                SetDim(target);
                done?.Invoke();
                return;
            }

            _fade = Tween.Custom(this, Dim, target, _fadeSeconds, (spotlight, value) => spotlight.SetDim(value), useUnscaledTime: true);
            if (done != null) _fade.OnComplete(done);
        }

        private void SetDim(float dim)
        {
            Dim = dim;
            Shader.SetGlobalFloat(DimId, dim);
        }

        // Every spotlit renderer back on its own layer.
        private void Restore()
        {
            foreach (KeyValuePair<GameObject, int> spotlit in _spotlit)
            {
                if (spotlit.Key != null) spotlit.Key.layer = spotlit.Value;
            }

            _spotlit.Clear();
        }

        private int RequireLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                throw new System.InvalidOperationException($"[SceneSpotlight] '{name}': the project has no '{layerName}' layer. Add it in the Tags and Layers settings.");
            }

            return layer;
        }
    }
}
