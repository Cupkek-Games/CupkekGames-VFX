using UnityEngine;
using Linework.WideOutline;
using System.Collections.Generic;
using System;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

namespace CupkekGames.VFX
{
    public class WideOutlineManager : OutlineManager
    {
        [SerializeField] private WideOutlineSettings _settings;

        public override void SetSharedWidth(float value)
        {
            _settings.sharedWidth = value;
        }

        public override void SetWidth(float value, int outlineIndex)
        {
            int settingsIndex = outlineIndex + 1;
            if (settingsIndex < 0 || settingsIndex >= _settings.Outlines.Count)
            {
                Debug.LogError($"[WideOutlineManager] SetWidth: outlineIndex {outlineIndex} is out of range (settings has {_settings.Outlines.Count} outlines, need index {settingsIndex})", this);
                return;
            }
            _settings.Outlines[settingsIndex].width = value;
        }

        public override void SetColor(Color color, int outlineIndex)
        {
            // first index is masking
            _settings.Outlines[outlineIndex + 1].color = color;
        }

        public override void RegisterServices()
        {
            ServiceLocator.Register(this);
        }

        public override void UnregisterServices()
        {
            ServiceLocator.Remove(this);
        }
    }
}