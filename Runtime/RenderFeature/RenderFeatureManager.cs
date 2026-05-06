#if PRIMETWEEN_INSTALLED && UNITASK_INSTALLED
using System.Collections.Generic;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CupkekGames.VFX
{
    public class RenderFeatureManager : ServiceProvider
    {
        private const string TAG_DARK_TRANSPARENT = "DarkTransparent";

        private Dictionary<GameObject, int> _gameObjects;
        private LayerMask _darkenLayer;
        private LayerMask _darkTransparentLayer;

        [SerializeField] private float _alpha;
        [SerializeField] private float _fadeDuration;
        [SerializeField] private Material[] _darkenMaterials;

        private List<Tween> _activeTweens = new List<Tween>();
        private CancellationTokenSource _cancellationTokenSource;

        protected override void Awake()
        {
            _gameObjects = new Dictionary<GameObject, int>();
            _darkenLayer = LayerMask.NameToLayer("Dark");
            _darkTransparentLayer = LayerMask.NameToLayer("DarkTransparent");

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CancelActiveTweens();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        public override void RegisterServices()
        {
            ServiceLocator.Register(this);
        }

        public override void UnregisterServices()
        {
            ServiceLocator.Remove(this);
        }

        public void Register(GameObject[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject != null && !_gameObjects.ContainsKey(gameObject))
                {
                    _gameObjects[gameObject] = gameObject.layer;
                }
            }
        }

        public void Register(ICollection<GameObject> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject != null && !_gameObjects.ContainsKey(gameObject))
                {
                    _gameObjects[gameObject] = gameObject.layer;
                }
            }
        }

        public void Register(GameObject gameObject)
        {
            if (gameObject != null && !_gameObjects.ContainsKey(gameObject))
            {
                _gameObjects[gameObject] = gameObject.layer;
            }
        }

        public void Register(GameObject gameObject, bool withChildren)
        {
            RegistrationScope scope = withChildren ? RegistrationScope.SelfAndChildren : RegistrationScope.Self;
            List<GameObject> registeredObjects = new List<GameObject>();
            RenderFeatureRegister.CollectGameObjects(registeredObjects, scope, gameObject);

            foreach (var registeredObject in registeredObjects)
            {
                if (registeredObject != null && !_gameObjects.ContainsKey(registeredObject))
                {
                    _gameObjects[registeredObject] = registeredObject.layer;
                }
            }
        }

        public void Unregister(GameObject[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                _gameObjects.Remove(gameObject);
            }
        }

        public void Unregister(ICollection<GameObject> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                _gameObjects.Remove(gameObject);
            }
        }

        public void Unregister(GameObject gameObject)
        {
            _gameObjects.Remove(gameObject);
        }

        public void Unregister(GameObject gameObject, bool withChildren)
        {
            RegistrationScope scope = withChildren ? RegistrationScope.SelfAndChildren : RegistrationScope.Self;
            List<GameObject> registeredObjects = new List<GameObject>();
            RenderFeatureRegister.CollectGameObjects(registeredObjects, scope, gameObject);

            foreach (var registeredObject in registeredObjects)
            {
                _gameObjects.Remove(registeredObject);
            }
        }

        public async UniTask DarkenEverythingExceptAsync(ICollection<GameObject> except, bool fade)
        {
            ClearNullObjects();

            foreach (var kvp in _gameObjects)
            {
                var gameObject = kvp.Key;

                // Skip objects in the exception list
                if (except.Contains(gameObject))
                {
                    continue;
                }

                // Choose layer based on tag
                int targetLayer = gameObject.CompareTag(TAG_DARK_TRANSPARENT) ? _darkTransparentLayer : _darkenLayer;

                // Only change layer if it's not already at the target layer
                if (gameObject.layer != targetLayer)
                {
                    gameObject.layer = targetLayer;
                }
            }

            if (fade)
            {
                // Fade in the darken materials
                await FadeInDarkenMaterialsAsync();
            }
        }

        public async UniTask UnDarkenEverythingAsync(bool fade)
        {
            if (fade)
            {
                // First fade out the darken materials
                await FadeOutDarkenMaterialsAsync();
            }

            // Then change the layers
            ClearNullObjects();

            foreach (var kvp in _gameObjects)
            {
                var gameObject = kvp.Key;
                var originalLayer = kvp.Value;

                if (gameObject.layer == _darkenLayer || gameObject.layer == _darkTransparentLayer)
                {
                    gameObject.layer = originalLayer;
                }
            }
        }

        public async UniTask UnDarkenAsync(ICollection<GameObject> gameObjects)
        {
            // Then restore the layers to their original state
            var defaultLayer = LayerMask.NameToLayer("Default");
            foreach (var gameObject in gameObjects)
            {
                if (gameObject.layer == _darkenLayer || gameObject.layer == _darkTransparentLayer)
                {
                    if (_gameObjects.TryGetValue(gameObject, out int originalLayer))
                    {
                        gameObject.layer = originalLayer;
                    }
                    else
                    {
                        gameObject.layer = defaultLayer;
                    }
                }
            }
        }

        public async UniTask UnDarkenAsync(GameObject gameObject, bool withChildren)
        {
            // Then change the layers
            RegistrationScope scope = withChildren ? RegistrationScope.SelfAndChildren : RegistrationScope.Self;
            List<GameObject> registeredObjects = new List<GameObject>();
            RenderFeatureRegister.CollectGameObjects(registeredObjects, scope, gameObject);

            var defaultLayer = LayerMask.NameToLayer("Default");

            foreach (var registeredObject in registeredObjects)
            {
                if (registeredObject.layer == _darkenLayer || registeredObject.layer == _darkTransparentLayer)
                {
                    if (_gameObjects.TryGetValue(registeredObject, out int originalLayer))
                    {
                        registeredObject.layer = originalLayer;
                    }
                    else
                    {
                        registeredObject.layer = defaultLayer;
                    }
                }
            }
        }

        public void ClearNullObjects()
        {
            var keysToRemove = new List<GameObject>();

            foreach (var kvp in _gameObjects)
            {
                if (kvp.Key == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _gameObjects.Remove(key);
            }
        }

        private void CancelActiveTweens()
        {
            foreach (var tween in _activeTweens)
            {
                tween.Stop();
            }

            _activeTweens.Clear();
        }

        private void CancelAndCreateNewToken()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Fades out the darken materials to 0 alpha
        /// </summary>
        public async UniTask FadeOutDarkenMaterialsAsync()
        {
            if (_darkenMaterials == null || _darkenMaterials.Length == 0) return;

            CancelAndCreateNewToken();
            CancelActiveTweens();

            foreach (var material in _darkenMaterials)
            {
                if (material != null)
                {
                    Color currentColor = material.color;
                    Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
                    var tween = Tween.MaterialColor(material, currentColor, targetColor, _fadeDuration);
                    _activeTweens.Add(tween);
                }
            }

            await UniTask.Delay((int)(_fadeDuration * 1000), cancellationToken: _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Fades in the darken materials to the target alpha value
        /// </summary>
        public async UniTask FadeInDarkenMaterialsAsync()
        {
            if (_darkenMaterials == null || _darkenMaterials.Length == 0) return;

            CancelAndCreateNewToken();
            CancelActiveTweens();

            foreach (var material in _darkenMaterials)
            {
                if (material != null)
                {
                    Color currentColor = material.color;
                    Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, _alpha);
                    var tween = Tween.MaterialColor(material, currentColor, targetColor, _fadeDuration);
                    _activeTweens.Add(tween);
                }
            }

            await UniTask.Delay((int)(_fadeDuration * 1000), cancellationToken: _cancellationTokenSource.Token);
        }
    }
}
#endif
