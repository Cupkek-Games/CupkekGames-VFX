using System.Collections.Generic;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using UnityEngine;

namespace CupkekGames.VFX
{
    public enum RegistrationScope
    {
        Self,
        SelfAndChildren,
        ChildrenOnly
    }

    public class RenderFeatureRegister : MonoBehaviour
    {
        private const string TAG_NEVER_DARKEN = "NeverDarken";

        [Header("Registration Settings")] [SerializeField]
        private RegistrationScope _scope = RegistrationScope.SelfAndChildren;

        // References
        private RenderFeatureManager _manager;

        // State
        private List<GameObject> _registeredObjects = new List<GameObject>();

        private void Awake()
        {
            _manager = ServiceLocator.Get<RenderFeatureManager>();
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Unregister();
        }

        public void Register()
        {
            _registeredObjects.Clear();
            CollectGameObjects(_registeredObjects, _scope, gameObject);

            if (_registeredObjects.Count > 0)
            {
                _manager.Register(_registeredObjects);
            }
        }

        public void Unregister()
        {
            if (_registeredObjects.Count > 0)
            {
                _manager.Unregister(_registeredObjects);
            }

            _registeredObjects.Clear();
        }

        public static void CollectGameObjects(List<GameObject> collection, RegistrationScope scope,
            GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            switch (scope)
            {
                case RegistrationScope.Self:
                    if (!gameObject.CompareTag(TAG_NEVER_DARKEN))
                    {
                        collection.Add(gameObject);
                    }

                    break;

                case RegistrationScope.SelfAndChildren:
                    if (!gameObject.CompareTag(TAG_NEVER_DARKEN))
                    {
                        collection.Add(gameObject);
                    }

                    CollectChildren(gameObject.transform, collection);
                    break;

                case RegistrationScope.ChildrenOnly:
                    CollectChildren(gameObject.transform, collection);
                    break;
            }
        }

        public static void CollectChildren(Transform parent, List<GameObject> collection)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (!child.gameObject.CompareTag(TAG_NEVER_DARKEN))
                {
                    collection.Add(child.gameObject);
                }

                CollectChildren(child, collection);
            }
        }
    }
}
