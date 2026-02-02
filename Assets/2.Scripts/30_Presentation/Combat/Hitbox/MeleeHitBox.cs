using System;
using UnityEngine;
using Hourbound.Presentation.Combat;
using Hourbound.Presentation.Combat.Hitbox;

namespace Hourbound.Presentation.Combat.Hitbox
{
    // 적의 근접 히트박스
    // - Trigger로 겹치는 순간 플레이어에게 피격 시도를 보낸다.
    // - 같은 공격에서 여러 번 맞는 것을 막기 위해 1회만 처리한다. 
    public sealed class MeleeHitBox : MonoBehaviour
    {
        [Header("출처(로그/디버그용)")]
        [SerializeField] private string sourceName = "적_근접";
        
        private bool _hasHit;
        
        // 공격 시작 시 호출(재사용 가능)
        public void Begin()
        {
            _hasHit = false;
            gameObject.SetActive(true);
        }
        
        // 공격 종료 시 호출
        public void End()
        {
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {  
            if (_hasHit) return;
            
            // 플레이어 히트박스인지 확인
            var hurtbox = other.GetComponent<PlayerHurtbox>();
            if (hurtbox == null) return;
            
            // 플레이어 루트에 있는 HitReceiver 찾기
            var receiver = other.GetComponentInParent<PlayerHitReceiver>();
            if (receiver == null) return;
            
            _hasHit = true;
            receiver.ReceiveHit(sourceName);
        }
    }
}