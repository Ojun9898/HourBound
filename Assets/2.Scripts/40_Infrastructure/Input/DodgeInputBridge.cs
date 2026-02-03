using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Player;

namespace Hourbound.Infrastructure.Input
{
    public sealed class DodgeInputBridge : MonoBehaviour
    {
        [SerializeField] private DodgeController controller;
        [SerializeField] private PlayerActionGate gate;
        [SerializeField] private MoveInputCache moveCache;

        [Header("Input Actions")] 
        [SerializeField] private InputActionReference dodgeAction;  // Button
        
        private void OnEnable()
        {
            if (dodgeAction != null)
            {
                dodgeAction.action.performed += OnDodge;
                dodgeAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (dodgeAction != null)
            {
                dodgeAction.action.performed -= OnDodge;
                dodgeAction.action.Disable();
            }
        }
        
        private void OnDodge(InputAction.CallbackContext ctx)
        {
            if (controller == null) return;
            if (gate != null && gate.IsBlocked(InputChannel.Gameplay)) return;
            
            Vector2 move = moveCache != null ? moveCache.Value : Vector2.zero;
            controller.RequestDodge(move);
        }
    }
}