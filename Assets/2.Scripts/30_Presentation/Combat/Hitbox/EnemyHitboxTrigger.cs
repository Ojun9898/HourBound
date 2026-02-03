using System;
using UnityEngine;

namespace Hourbound.Presentation.Combat.Hitbox
{
    /// <summary>
    /// 피격 시도 검증용
    /// EnemyHitBox 트리거가 플레이어 Hurtbox를 건드리면
    /// PlayerHitReceiver.ReceiverHit() 호출
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class EnemyHitboxTrigger : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool logOnHit = true;
        
        [Header("Filter")]
        [Tooltip("비워두면 필터 없이 처리, 설정하면 해당 레이어만 처리.")]
        [SerializeField] private LayerMask targetLayers;
        
        private Collider _trigger;

        private void Reset()
        {
            _trigger = GetComponent<Collider>();
            if (_trigger != null) _trigger.isTrigger = true;
        }

        private void Awake()
        {
            _trigger = GetComponent<Collider>();
            if (_trigger == null)
            {
                Debug.LogError("EnemyHitboxTrigger : Collider가 필요합니다.", this);
                enabled = false;
                return;
            }
            
            _trigger.isTrigger = true;
            _trigger.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // 레이어 필터
            if (targetLayers.value != 0)
            {
                int otherLayerMask = 1 << other.gameObject.layer;
                if ((targetLayers.value & otherLayerMask) == 0) return;
            }
            
            // 플레이어 Receiver 찾기
            var receiver = other.GetComponentInParent<PlayerHitReceiver>();
            if (receiver == null) return;
            
            if (logOnHit)
                Debug.Log($"[EnemyHitboxTrigger] Hit : {other.name} -> Receiver on {receiver.gameObject.name}", this);
            
            receiver.ReceiveHit(gameObject);
        }
        
    }
}