using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hourbound.Presentation.Enemy.Combat
{
    /// <summary>
    /// 테스트용 적 공격 트리거 컨트롤러,
    /// - 키 입력 또는 자동 타이머로 Animator의 Attack 트리거를 발생시킨다.
    /// - 실제 AI로 교체하기 전까지 사용
    /// </summary>
    public sealed class EnemyAttackTestController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Animator animator;
        
        [Header("Animator Param")]
        [SerializeField] private string attackTriggerName = "Attack";
        
        [Header("Mode")]
        [SerializeField] private bool manualKey = true;
        [SerializeField] private Key manualKeyCode = Key.K;
        
        [SerializeField] private bool autoLoop = false;
        [Min(0.1f)] [SerializeField] private float autoInterval = 1.25f;
        
        [Header("Cooldown")]
        [Min(0f)] [SerializeField] private float attackCooldown = 0.6f;
        
        [Header("Debug")]
        [SerializeField] private bool log;
        
        private int _attackTriggerHash;
        private float _nextAttackAt;
        private float _nextAutoAt;

        private void Reset()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Awake()
        {
            if (animator == null)
            {
                Debug.LogError("EnemyAttackTestController : Animator가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            
            _attackTriggerHash = Animator.StringToHash(attackTriggerName);
            _nextAutoAt = UnityEngine.Time.timeSinceLevelLoad + autoInterval;
        }

        private void Update()
        {
            float now = UnityEngine.Time.timeSinceLevelLoad;
            
            bool wantAttack = false;

            if (manualKey)
            {
                var kb = Keyboard.current;
                if (kb != null && kb[manualKeyCode].wasPressedThisFrame)
                    wantAttack = true;
            }

            if (!wantAttack && autoLoop && now >= _nextAutoAt)
            {
                wantAttack = true;
                _nextAutoAt = now + autoInterval;
            }
            
            if (!wantAttack) return;
            
            if (now < _nextAttackAt)
                return;
            
            _nextAttackAt = now + attackCooldown;
            
            animator.ResetTrigger(_attackTriggerHash);
            animator.SetTrigger(_attackTriggerHash);
            
            if (log)
                Debug.Log($"[EnemyAttackTest] Attack trigger fired({attackTriggerName})", this);
        }
    }
}