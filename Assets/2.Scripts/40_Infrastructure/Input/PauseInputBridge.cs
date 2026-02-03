using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Player;

namespace Hourbound.Infrastructure.Input
{
    public sealed class PauseInputBridge : MonoBehaviour
    {
        [SerializeField] private PlayerActionGate gate;
        [SerializeField] private InputActionReference pauseAction;
        
        public Action PausePressed;

        private void OnEnable()
        {
            if (pauseAction == null) return;
            pauseAction.action.performed += OnPause;
            pauseAction.action.Enable();
        }

        private void OnDisable()
        {
            if (pauseAction == null) return;
            pauseAction.action.performed -= OnPause;
            pauseAction.action.Disable();
        }

        private void OnPause(InputAction.CallbackContext ctx)
        {
            // System 채널은 항상 통과(필요하면 gate.IsBlocked(System)으로 확장 가능)
            PausePressed?.Invoke();
        }
    }
}