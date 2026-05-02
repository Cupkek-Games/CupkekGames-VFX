using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Fadeables;

namespace CupkekGames.VFX
{
    /// <summary>
    /// Represents a shader property that can be faded with its original value and fade range
    /// </summary>
    [Serializable]
    public class ShaderPropertyFadeData
    {
        [SerializeField] public string PropertyName;
        [SerializeField] public ShaderPropertyType PropertyType;

        // Float fade values
        [SerializeField] public float FadeOutValue;
        [SerializeField] public float FadeInValue;

        // Color fade values
        [SerializeField] public Color FadeOutColor = new Color(1, 1, 1, 0);
        [SerializeField] public Color FadeInColor = Color.white;

        // Vector fade values
        [SerializeField] public Vector4 FadeOutVector = Vector4.zero;
        [SerializeField] public Vector4 FadeInVector = Vector4.one;

        // Store original values per renderer/material (runtime only, not serialized)
        [NonSerialized]
        private Dictionary<int, object> _originalValues = new Dictionary<int, object>();

        public ShaderPropertyFadeData(string propertyName, ShaderPropertyType propertyType, float fadeOutValue, float fadeInValue)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            FadeOutValue = fadeOutValue;
            FadeInValue = fadeInValue;
        }

        public ShaderPropertyFadeData(string propertyName, Color fadeOutColor, Color fadeInColor)
        {
            PropertyName = propertyName;
            PropertyType = ShaderPropertyType.Color;
            FadeOutColor = fadeOutColor;
            FadeInColor = fadeInColor;
        }

        public ShaderPropertyFadeData(string propertyName, Vector4 fadeOutVector, Vector4 fadeInVector)
        {
            PropertyName = propertyName;
            PropertyType = ShaderPropertyType.Vector;
            FadeOutVector = fadeOutVector;
            FadeInVector = fadeInVector;
        }

        public void StoreOriginalValue(int materialHash, object value)
        {
            _originalValues ??= new Dictionary<int, object>();
            _originalValues[materialHash] = value;
        }

        public bool TryGetOriginalValue(int materialHash, out object value)
        {
            return _originalValues.TryGetValue(materialHash, out value);
        }

        public void ClearOriginalValues()
        {
            _originalValues.Clear();
        }
    }

    public enum ShaderPropertyType
    {
        Float,
        Color,
        ColorAlpha,
        Vector
    }

    /// <summary>
    /// Generic component for fading any shader property or multiple properties at once.
    /// Similar to Dissolvable but supports runtime addition/removal of properties.
    /// </summary>
    public class ShaderPropertyFade : FadeableMono
    {
        [SerializeField] private List<ShaderPropertyFadeData> _shaderProperties = new List<ShaderPropertyFadeData>();

        private Renderer[] _renderers;
        private Dictionary<Material, int> _materialHashes = new Dictionary<Material, int>();

        protected override void Awake()
        {
            base.Awake();

            _renderers = GetComponentsInChildren<Renderer>();

            // Store original values for all materials
            StoreOriginalValues();

            // Subscribe to Fadeable events
            Fadeable.OnApply += Apply;
        }

        protected override void OnDestroy()
        {
            if (Fadeable != null)
            {
                Fadeable.OnApply -= Apply;
            }
        }

        /// <summary>
        /// Store original values for all materials and properties
        /// </summary>
        private void StoreOriginalValues()
        {
            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Material mat = materials[j];
                    if (mat == null) continue;

                    int materialHash = mat.GetInstanceID();
                    _materialHashes[mat] = materialHash;

                    foreach (ShaderPropertyFadeData propertyData in _shaderProperties)
                    {
                        if (!mat.HasProperty(propertyData.PropertyName))
                            continue;

                        object originalValue = GetPropertyValue(mat, propertyData);
                        if (originalValue != null)
                        {
                            propertyData.StoreOriginalValue(materialHash, originalValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply fade value to all shader properties
        /// </summary>
        private void Apply()
        {
            float fadeValue = Fadeable.Value;

            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Material mat = materials[j];
                    if (mat == null) continue;

                    int materialHash = mat.GetInstanceID();

                    foreach (ShaderPropertyFadeData propertyData in _shaderProperties)
                    {
                        if (!mat.HasProperty(propertyData.PropertyName))
                            continue;

                        ApplyPropertyFade(mat, propertyData, fadeValue, materialHash);
                    }
                }
            }
        }

        /// <summary>
        /// Apply fade to a specific property on a material
        /// </summary>
        private void ApplyPropertyFade(Material mat, ShaderPropertyFadeData propertyData, float fadeValue, int materialHash)
        {
            switch (propertyData.PropertyType)
            {
                case ShaderPropertyType.Float:
                    float lerpValue = Mathf.Lerp(propertyData.FadeOutValue, propertyData.FadeInValue, fadeValue);
                    mat.SetFloat(propertyData.PropertyName, lerpValue);
                    break;

                case ShaderPropertyType.Color:
                    Color targetColor = Color.Lerp(propertyData.FadeOutColor, propertyData.FadeInColor, fadeValue);
                    mat.SetColor(propertyData.PropertyName, targetColor);
                    break;

                case ShaderPropertyType.ColorAlpha:
                    if (propertyData.TryGetOriginalValue(materialHash, out object origColorObj) && origColorObj is Color origColor)
                    {
                        float targetAlpha = Mathf.Lerp(propertyData.FadeOutValue, propertyData.FadeInValue, fadeValue);
                        origColor.a = targetAlpha;
                        mat.SetColor(propertyData.PropertyName, origColor);
                    }
                    break;

                case ShaderPropertyType.Vector:
                    Vector4 targetVector = Vector4.Lerp(propertyData.FadeOutVector, propertyData.FadeInVector, fadeValue);
                    mat.SetVector(propertyData.PropertyName, targetVector);
                    break;
            }
        }

        /// <summary>
        /// Get the current value of a shader property
        /// </summary>
        private object GetPropertyValue(Material mat, ShaderPropertyFadeData propertyData)
        {
            if (!mat.HasProperty(propertyData.PropertyName))
                return null;

            switch (propertyData.PropertyType)
            {
                case ShaderPropertyType.Float:
                    return mat.GetFloat(propertyData.PropertyName);

                case ShaderPropertyType.Color:
                    return mat.GetColor(propertyData.PropertyName);

                case ShaderPropertyType.ColorAlpha:
                    return mat.GetColor(propertyData.PropertyName);

                case ShaderPropertyType.Vector:
                    return mat.GetVector(propertyData.PropertyName);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Add a shader property to fade at runtime
        /// </summary>
        public void AddProperty(string propertyName, ShaderPropertyType propertyType, float fadeOutValue = 0f, float fadeInValue = 1f)
        {
            // Check if property already exists
            foreach (ShaderPropertyFadeData existing in _shaderProperties)
            {
                if (existing.PropertyName == propertyName)
                {
                    Debug.LogWarning($"[ShaderPropertyFade] Property '{propertyName}' already exists. Use RemoveProperty first or UpdateProperty to modify it.");
                    return;
                }
            }

            ShaderPropertyFadeData newProperty = new ShaderPropertyFadeData(propertyName, propertyType, fadeOutValue, fadeInValue);
            _shaderProperties.Add(newProperty);

            // Store original values for the new property
            StoreOriginalValuesForProperty(newProperty);
        }

        /// <summary>
        /// Remove a shader property from the fade list at runtime
        /// </summary>
        public bool RemoveProperty(string propertyName)
        {
            for (int i = _shaderProperties.Count - 1; i >= 0; i--)
            {
                if (_shaderProperties[i].PropertyName == propertyName)
                {
                    _shaderProperties[i].ClearOriginalValues();
                    _shaderProperties.RemoveAt(i);
                    return true;
                }
            }

            Debug.LogWarning($"[ShaderPropertyFade] Property '{propertyName}' not found.");
            return false;
        }

        /// <summary>
        /// Update fade values for an existing property
        /// </summary>
        public bool UpdateProperty(string propertyName, float fadeOutValue, float fadeInValue)
        {
            foreach (ShaderPropertyFadeData propertyData in _shaderProperties)
            {
                if (propertyData.PropertyName == propertyName)
                {
                    propertyData.FadeOutValue = fadeOutValue;
                    propertyData.FadeInValue = fadeInValue;
                    return true;
                }
            }

            Debug.LogWarning($"[ShaderPropertyFade] Property '{propertyName}' not found.");
            return false;
        }

        /// <summary>
        /// Store original values for a specific property
        /// </summary>
        private void StoreOriginalValuesForProperty(ShaderPropertyFadeData propertyData)
        {
            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Material mat = materials[j];
                    if (mat == null) continue;

                    if (!mat.HasProperty(propertyData.PropertyName))
                        continue;

                    int materialHash = mat.GetInstanceID();
                    object originalValue = GetPropertyValue(mat, propertyData);
                    if (originalValue != null)
                    {
                        propertyData.StoreOriginalValue(materialHash, originalValue);
                    }
                }
            }
        }

        /// <summary>
        /// Get all registered property names
        /// </summary>
        public List<string> GetPropertyNames()
        {
            List<string> names = new List<string>();
            foreach (ShaderPropertyFadeData propertyData in _shaderProperties)
            {
                names.Add(propertyData.PropertyName);
            }
            return names;
        }

        /// <summary>
        /// Clear all properties
        /// </summary>
        public void ClearAllProperties()
        {
            foreach (ShaderPropertyFadeData propertyData in _shaderProperties)
            {
                propertyData.ClearOriginalValues();
            }
            _shaderProperties.Clear();
        }
    }
}
