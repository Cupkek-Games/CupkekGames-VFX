using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.VFX
{
    public class OutlineReference : MonoBehaviour
    {
        private Dictionary<OutlineManager, HashSet<int>> _controller = new(); // controller to index list

        private void OnDestroy()
        {
            foreach (var kvp in _controller)
            {
                foreach (int i in kvp.Value)
                {
                    kvp.Key.RemoveOutline(gameObject, i);
                }
            }
        }

        public void Add(OutlineManager controller, int outlineIndex)
        {
            if (_controller.TryGetValue(controller, out HashSet<int> indexes))
            {
                indexes.Add(outlineIndex);
            }
            else
            {
                _controller.Add(controller, new HashSet<int>() { outlineIndex });
            }
        }
    }
}