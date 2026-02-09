using System;
using UnityEngine;
using Hourbound.Infrastructure.Input;
using Hourbound.Presentation.Combat;
using Hourbound.Presentation.Player;
using Hourbound.Presentation.Player.Combat;

namespace Hourbound.Presentation.Player.State
{
    /// <summary>
    /// 기능별 컴포넌트를 유지한 채, 상태별 우선순위/상호배타 규칙을 한 곳에서 고정하는 FSM 오케스트레이터
    /// - Dodge/Hurt/Attack 중복 입력과 전이 꼬임을 줄이는 목적.
    /// - 현재 Attack 종료는 타이머 기반(임시). 이후 애니 이벤트 기반으로 교체한다.
    /// </summary>
    public sealed class PlayerFsmOrchestrator : MonoBehaviour
    {
        public enum State { Locomotion, Attack, Dodge, Hurt }
        
        [Header("Input Bridges")]
        [SerializeField] private PlayerMoveInputBridge moveBridge;
        [SerializeField] private DodgeInputBridge dodgeBridge;
        [SerializeField] private PlayerAttackInputBridge attackBridge;
        
        [Header("System")]
        [SerializeField] private DodgeController dodge;
        [SerializeField] private PlayerHitReceiver hitReceiver;
        
        [Header("Attack Excution (Attack Trigger)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string attackTrigger = "Attack";
        private int _attackHash;
        
        [Header("Turning")]
        [Min(0f)] [SerializeField] private float hurtLockSeconds = 0.20f;
        
        private State _state = State.Locomotion;
        private float _lockUntilUnscaled = -1f;

        private void Awake()
        {
            if (moveBridge == null) moveBridge = GetComponentInChildren<PlayerMoveInputBridge>(true);
            if (dodgeBridge == null) dodgeBridge = GetComponentInChildren<DodgeInputBridge>(true);
            if (attackBridge == null) attackBridge = GetComponentInChildren<PlayerAttackInputBridge>(true);
            
            if (dodge == null) dodge = GetComponentInChildren<DodgeController>(true);
            if (hitReceiver == null) hitReceiver = GetComponentInChildren<PlayerHitReceiver>(true);
            
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
        }

        private void OnEnable()
        {
            if (attackBridge != null) attackBridge.AttackRequested += OnAttackRequested;

            if (dodge != null)
            {
                dodge.DodgeStarted += OnDodgeStarted;
                dodge.DodgeEnded += OnDodgeEnded;
            }
            
            if (hitReceiver != null)
                hitReceiver.Damaged += OnDamaged;
        }
        
        private void OnDisable()
        {
            if (attackBridge != null) attackBridge.AttackRequested -= OnAttackRequested;

            if (dodge != null)
            {
                dodge.DodgeStarted -= OnDodgeStarted;
                dodge.DodgeEnded -= OnDodgeEnded;
            }
            
            if (hitReceiver != null)
                hitReceiver.Damaged -= OnDamaged;
        }

        private void Update()
        {
            if ((_state == State.Attack || _state == State.Hurt) && UnityEngine.Time.unscaledTime >= _lockUntilUnscaled)
                SetState(State.Locomotion);
        }

        private void OnAttackRequested()
        {
            if (_state != State.Locomotion) return;
            if (animator == null) return;
            
            // 공격 시작 : 즉시 이동/입력을 잠그고, 애니 트리거 발사
            SetState(State.Attack);
            animator.ResetTrigger(_attackHash);
            animator.SetTrigger(_attackHash);
        }
        
        private void OnDodgeStarted()
        {
            SetState(State.Dodge);
        }

        private void OnDodgeEnded()
        {
            // Hurt가 우선이면 Hurt 유지
            if (_state == State.Hurt) return;
            // Attack 중이면 Attack 유지(공격 종료는 애니메이션 이벤트로만)
            if (_state == State.Attack) return;
            
            SetState(State.Locomotion);
        }

        private void OnDamaged()
        {
            // 피격 우선
            SetState(State.Hurt);
            _lockUntilUnscaled = UnityEngine.Time.unscaledTime + hurtLockSeconds;
        }
        
        /// <summary>
        /// (애니메이션 이벤트에서 호출) 공격 판정 윈도우 시작.
        /// 지금은 로깅/확장 포인트로만 둔다.
        /// </summary>
        public void NotifyAttackWindowBegan() {}
        
        /// <summary>
        /// (애니메이션 이벤트에서 호출) 공격 판정 윈도우 종료
        /// </summary>
        public void NotifyAttackWindowEnded() {}

        /// <summary>
        /// (애니메이션 이벤트에서 호출) 공격 애니메이션 종료 -> Attack 상태 종료.
        /// </summary>
        public void NotifyAttackFinished()
        {
            if (_state != State.Attack) return;
            
            //  Dodge/Hurt가 우선이면 그쪽 유지 (보통 Attack 도중 피격이면 Hurt로 바뀌어있음)
            if (_state == State.Hurt || _state == State.Dodge) return;
            
            SetState(State.Locomotion);
        }

        private void SetState(State next)
        {
            if (_state == next) return;
            _state = next;
            
            bool allowMove = (next == State.Locomotion);
            bool allowDodge = (next == State.Dodge);
            bool allowAttack = (next == State.Attack);
            
            if (moveBridge != null) moveBridge.enabled = allowMove;
            if (dodgeBridge != null) dodgeBridge.enabled = allowDodge;
            if (attackBridge != null) attackBridge.enabled = allowAttack;
        }
    }
}