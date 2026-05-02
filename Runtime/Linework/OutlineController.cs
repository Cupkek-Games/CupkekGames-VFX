using UnityEngine;
using CupkekGames.Fadeables;

namespace CupkekGames.VFX
{
    public class OutlineController
    {
        public Fadeable Fadeable;
        private OutlineManager _controller;
        public int OutlineIndex;
        public Color ColorStart = Color.white;
        public Color ColorEnd = Color.white;

        public OutlineController(OutlineManager parent, int outlineIndex)
        {
            _controller = parent;
            OutlineIndex = outlineIndex;
            Fadeable = new Fadeable(_controller);
        }

        public void Kill()
        {
            Fadeable.Kill();
        }

        public void OnEnable()
        {
            Fadeable.OnApply += OnFadeable;
        }

        public void OnDisable()
        {
            Fadeable.OnApply -= OnFadeable;
        }

        private void OnFadeable()
        {
            // Set width
            if (OutlineIndex < 0)
            {
                _controller.SetSharedWidth(Fadeable.Value);
            }
            else
            {
                _controller.SetWidth(Fadeable.Value, OutlineIndex);

                if (ColorStart != ColorEnd)
                {
                    // Normalize fade value to 0-1 range for color interpolation
                    float t = Mathf.InverseLerp(Fadeable._out, Fadeable._in, Fadeable.Value);
                    _controller.SetColor(Color.Lerp(ColorStart, ColorEnd, t), OutlineIndex);
                }
                else
                {
                    _controller.SetColor(ColorStart, OutlineIndex);
                }
            }
        }
    }
}