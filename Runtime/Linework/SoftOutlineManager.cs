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