using System;
using UnityEngine;
using Hourbound.Presentation.Enemy.Combat;

namespace Hourbound.Presentation.Enemy.Movement
{
    /// <summary>
    /// 아주 단순한 추적 이동 컴포넌트(프로토타입)
    /// - target을 향해 이동
    /// - stopDistance(공격 범위) 안이면 정지
    /// - 공격 중(EnemyMeleeAttackBrain 잠금 중)이면 정지
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyChaseMover : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform target;
        [SerializeField] private EnemyMeleeAttackBrain attackBrain;
        
        [Header("Move")]
        [Min(0f)] [SerializeField] private float moveSpeed = 2.2f;
        
        [Tooltip("이 거리 안이면 보통 멈춤(보통 attackRange보다 살짝 크게)")]
        [Min(0f)] [SerializeField] private float stopDistance = 2.4f;
        
        [Header("Option")]
        [SerializeField] private bool stopWhileAttacking = true;
        
        [Header("Debug")]
        [SerializeField] private bool log;
        
        private Rigidbody _rb;

        private void Reset()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            
            if (_rb == null)
            {
                Debug.LogError("EnemyChaseMover : Rigidbody를 찾지 못했습니다.", this);
                enabled = false;
                return;
            }
        }

        private void FixedUpdate()
        {
            if (target == null) return;
            
            // 공격 중이면 정지
            if (stopWhileAttacking && attackBrain != null && attackBrain.IsLocked)
            {
                Vector3 v = _rb.linearVelocity;
                v.x = 0f;
                v.z = 0f;
                _rb.linearVelocity = v;
                return;
                
            }
            
            Vector3 pos = _rb.position;
            Vector3 to = target.position - pos;
            to.y = 0f;
            
            float dist = to.magnitude;
            if (dist <= stopDistance)
            {
                // 정지 : 수평 속도만 0으로
                Vector3 v = _rb.linearVelocity;
                v.x = 0f;
                v.z = 0f;
                _rb.linearVelocity = v;
                return;
            }
            
            Vector3 dir = to / dist;
            Vector3 delta = dir * (moveSpeed * UnityEngine.Time.fixedDeltaTime);
            
            _rb.MovePosition(pos + delta);
            
            
            if (log)
                Debug.Log($"[Chase] dist = {dist:F2}", this);
        }
    }
}