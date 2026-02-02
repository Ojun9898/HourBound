using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Player;
using TMPro;

namespace Hourbound.Infrastructure.Input
{
    public sealed class DodgeInputBridge : MonoBehaviour
    {
        [SerializeField] private DodgeController controller;
        
        [Header("Input Action")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference dodgeAction;
        
        private Vector2 _cachedMove;

        private void OnEnable()
        {
            if (moveAction != null)
            {
                moveAction.action.performed += OnMove;
                moveAction.action.canceled += OnMove;
                moveAction.action.Enable();
            }

            if (dodgeAction != null)
            {
                dodgeAction.action.performed += OnDodge;
                dodgeAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action.performed -= OnMove;
                moveAction.action.canceled -= OnMove;
                moveAction.action.Disable();
            }

            if (dodgeAction != null)
            {
                dodgeAction.action.performed -= OnDodge;
                dodgeAction.action.Disable();
            }
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _cachedMove = ctx.ReadValue<Vector2>();
        }

        private void OnDodge(InputAction.CallbackContext ctx)
        {
            if (controller == null) return;
            controller.RequestDodge(_cachedMove);
        }
    }
}