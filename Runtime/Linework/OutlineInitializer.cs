using UnityEngine;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

namespace CupkekGames.VFX
{
    /// <summary>
    /// Sets outline widths and colors on Awake. Useful for resetting outlines on scene load.
    /// </summary>
    public class OutlineInitializer : MonoBehaviour
    {
        [System.Serializable]
        public struct OutlineSettings
        {
            public int index;
            public float width;
            public Color color;
        }

        [SerializeField] private OutlineType _outlineType;
        [SerializeField] private OutlineSettings[] _outlines;

        private void Awake()
        {
            OutlineManager manager = _outlineType == OutlineType.Wide
                ? ServiceLocator.Get<WideOutlineManager>()
                : ServiceLocator.Get<SoftOutlineManager>();

            if (manager == null)
            {
                Debug.LogError($"[OutlineInitializer] Failed to get OutlineManager for type {_outlineType}", this);
                return;
            }

            foreach (var outline in _outlines)
            {
                manager.SetWidth(outline.width, outline.index);
                manager.SetColor(outline.color, outline.index);
            }
        }
    }
}
