using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Player;

namespace Hourbound.Infrastructure.Input
{
    /// <summary>
    /// Input System의 Attack 액션을 받아 "공격 요청" 이벤트만 발행하는 브릿지.
    /// - 실제 공격 실행(히트박스/애니 트리거 등)은 FSM/컨트롤러가 처리한다.
    /// - Gate(GamePlay 차단) 적용 지점.
    /// </summary>
    public sealed class PlayerAttackInputBridge : MonoBehaviour
    {
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private PlayerActionGate gate;
        
        public event Action AttackRequested;

        private void OnEnable()
        {
            if (attackAction?.action == null) return;
            attackAction.action.performed += OnAttack;
            attackAction.action.Enable();
        }

        private void OnDisable()
        {
            if (attackAction?.action == null) return;
            attackAction.action.performed -= OnAttack;
            attackAction.action.Disable();
        }

        private void OnAttack(InputAction.CallbackContext _)
        {
            if (gate != null && gate.IsBlocked(InputChannel.Gameplay))
                return;
            
            AttackRequested?.Invoke();
        }
    }
}