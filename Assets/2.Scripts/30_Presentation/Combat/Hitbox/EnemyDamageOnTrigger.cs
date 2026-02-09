using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hourbound.Presentation.Combat.Hitbox
{
    /// <summary>
    /// Trigger Collider에서 적(EnemyHealth)을 찾아 데미지를 주는 히트박스용 컴포넌트
    /// - 같은 활성 구간에서 동일 대상 중복 히트 방지를 포함한다.
    /// - EnemyHealth는 TakeDamage(int) API를 사용한다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class EnemyDamageOnTrigger : MonoBehaviour
    {
        [Header("Damage")]
        [Min(0)] [SerializeField] private int damage = 5;
        
        [Header("Filter (sorted)")]
        [Tooltip("Enemy 레이어만 맞추고 싶으면 설정. 비워두면 모든 레이어 허용")]
        [SerializeField] private LayerMask enemyLayerMask = ~0;
        
        [Tooltip("특정 Tag만 맞추고 싶으면 입력 ")]
        [SerializeField] private string requireTag = "";
        
        private readonly HashSet<int> _hitTarget = new();

        private void OnEnable()
        {
            _hitTarget.Clear();
        }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsAllowed(other)) return;
            
            var health = other.GetComponentInParent<EnemyHealth>();
            if (health == null) return;
            
            int id = health.GetInstanceID();
            if (!_hitTarget.Add(id)) return; // 같은 활성 구간에서 중복 타격 방지
            
            health.TakeDamage(damage);
        }

        private bool IsAllowed(Collider other)
        {
            // LayerMask 필터
            int otherLayerBit = 1 << other.gameObject.layer;
            if ((enemyLayerMask.value & otherLayerBit) == 0) return false;
            
            // Tag 필터(옵션)
            if (!string.IsNullOrEmpty(requireTag) && !other.CompareTag(requireTag))
                return false;
            
            return true;
        }
    }
}