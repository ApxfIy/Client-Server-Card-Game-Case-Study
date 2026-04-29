using System;
using UnityEngine;

namespace WarGame.Client.Views
{
    public class LayoutSwitcher : MonoBehaviour
    {
        [Serializable]
        public struct LayoutEntry
        {
            public Transform target;
            public Transform verticalParent;
            public Transform horizontalParent;
        }

        [SerializeField] private LayoutEntry[] entries;

        private bool _isLandscape;

        private void Start()
        {
            _isLandscape = IsLandscape();
            ApplyLayout(_isLandscape);
        }

        private void Update()
        {
            var landscape = IsLandscape();
            if (landscape == _isLandscape) return;

            _isLandscape = landscape;
            ApplyLayout(landscape);
        }

        private void ApplyLayout(bool landscape)
        {
            foreach (var entry in entries)
            {
                var parent = landscape ? entry.horizontalParent : entry.verticalParent;
                entry.target.SetParent(parent, false);
            }
        }

        private static bool IsLandscape() => Screen.width > Screen.height;
    }
}
