using UnityEngine;
using Linework.SoftOutline;
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
    public class SoftOutlineManager : OutlineManager
    {
        [SerializeField] private SoftOutlineSettings _settings;

        // SoftOutline has one shared kernel size — no per-index width exists.
        public override bool SupportsPerIndexWidth => false;

        public override void SetSharedWidth(float value)
        {
            _settings.kernelSize = (int)(value + 0.5f);
        }

        public override void SetWidth(float value, int outlineIndex)
        {
            Debug.LogError($"[SoftOutlineManager] SetWidth is not supported because SoftOutline uses kernel size instead of width. Use SetSharedWidth to set the kernel size for all outlines.", this);
        }

        public override void SetColor(Color color, int outlineIndex)
        {
            // hard
            // _settings.sharedColor = color;

            if (outlineIndex < 0 || outlineIndex >= _settings.Outlines.Count)
            {
                Debug.LogError(
                    $"[SoftOutlineManager] SetColor index {outlineIndex} is out of range — " +
                    $"'{_settings.name}' has {_settings.Outlines.Count} outline(s). " +
                    "Fix the caller's outline entries or add outlines to the settings asset.", this);
                return;
            }

            // soft
            _settings.Outlines[outlineIndex].color = color;
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