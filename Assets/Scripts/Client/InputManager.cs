using System;
using UnityEngine;

namespace WarGame.Client
{
    // TODO extract IInputManager and create StandaloneInputManager and MobileInputManager
    public class InputManager : MonoBehaviour
    {
        public event Action OnInput;

        private bool _isEnabled;

        public void EnableInput() => _isEnabled = true;
        public void DisableInput() => _isEnabled = false;

        private void Update()
        {
            if (!_isEnabled) return;

            if (Input.GetMouseButtonDown(0))
                OnInput?.Invoke();
        }
    }
}