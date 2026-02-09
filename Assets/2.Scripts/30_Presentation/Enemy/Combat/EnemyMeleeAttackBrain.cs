using System;
using UnityEngine;

namespace Hourbound.Presentation.Enemy.Combat
{
    /// <summary>
    /// 아주 단순한 근접 공격 브레인(프로토타입용)
    /// - 플레이어가 사거리 안이면 Attack 트리거
    /// - 쿨다운 적용
    /// - 공격 중 잠금(Anim 상태/타이머 기반)
    /// </summary>
    public sealed class EnemyMeleeAttackBrain : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform target; // Player 루트
        [SerializeField] private Animator animator;
        
        [Header("Attack")]
        [SerializeField] private string attackTriggerName = "Attack";
        [Min(0.1f)] [SerializeField] private float attackRange = 1.0f;
        [Min(0f)] [SerializeField] private float attackCooldown = 0.8f;
        
        [Header("Attack Lock")]
        [Tooltip("Attack 트리거 후 잠금 시간(Clip 길이에 맞춰야 함.)")]
        [Min(0.05f)] [SerializeField] private float lockDuration = 0.4f;
        
        [Header("Rotate")]
        [SerializeField] private bool rotateToTarget = true;
        [Min(0f)] [SerializeField] private float rotationSpeedDegPerSec = 540f;
        
        [Header("Debug")]
        [SerializeField] private bool log;
        
        private int _attackTriggerHash;
        private float _nextAttackAt;
        private float _lockedUntil;
        
        public bool IsLocked => UnityEngine.Time.timeSinceLevelLoad < _lockedUntil;
        
        private static readonly int AttackTagHash = Animator.StringToHash("Attack");

        private void Reset()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Awake()
        {
            if (animator == null)
            {
                Debug.LogError("EnemyMeleeAttackBrain : Animator가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            
            _attackTriggerHash = Animator.StringToHash(attackTriggerName);
        }
        
        private void Update()
        {
            if (target == null) return;
            
            float now = UnityEngine.Time.timeSinceLevelLoad;

            if (IsInAttackState())
            {
                if (rotateToTarget) RotateTowards(target.position);
                return;
            }
            
            // 공격 중 잠금
            if (now < _lockedUntil)
            {
                if (rotateToTarget) RotateTowards(target.position);
                return;
            }
            
            // 거리 체크
            Vector3 a = transform.position;
            Vector3 b = target.position;
            a.y = 0f; b.y = 0f;
            float dist = Vector3.Distance(a, b);
            
            if (rotateToTarget) RotateTowards(target.position);
            
            if (dist > attackRange) return;
            if (now < _nextAttackAt) return;
            
            // 공격 트리거
            _nextAttackAt = now + attackCooldown;
            _lockedUntil = now + lockDuration;
            
            animator.ResetTrigger(_attackTriggerHash);
            animator.SetTrigger(_attackTriggerHash);
            
            if (log)
                Debug.Log($"[EnemyBrain] Attack! dist = {dist:F2}", this);
        }

        private bool IsInAttackState()
        {
            var s = animator.GetCurrentAnimatorStateInfo(0);
            return s.tagHash == AttackTagHash;
        }

        private void RotateTowards(Vector3 worldPos)
        {
            Vector3 dir = worldPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeedDegPerSec * UnityEngine.Time.deltaTime
                );
        }
    }
}