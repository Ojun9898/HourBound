using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Player;

namespace Hourbound.Infrastructure.Input
{
    public sealed class PlayerMoveInputBridge : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private InputActionReference moveAction; // Vector2
        [SerializeField] private PlayerActionGate gate;
        [SerializeField] private MoveInputCache cache;

        private void OnEnable()
        {
            if (moveAction == null) return;
            
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
            moveAction.action.Enable();
        }

        private void OnDisable()
        {
            if (moveAction == null) return;
            
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled -= OnMove;
            moveAction.action.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 v = ctx.ReadValue<Vector2>();

            if (gate != null && gate.IsBlocked(InputChannel.Gameplay))
                v = Vector2.zero;

            movement.SetMoveInput(v);
            if (cache != null) cache.Set(v);
        }

    }
}