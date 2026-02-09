using System;
using UnityEngine;

namespace Hourbound.Presentation.Player.Combat
{
    /// <summary>
    /// 공격 요쳥을 받아 히트박스를 일정 시간만 활성화하는 액티베이터.
    /// - 현재 단계는 "임시 전투 검증" 용도.
    /// TODO : 이후 애니 이벤트 기반으로 교체
    /// </summary>
    public sealed class PlayerAttackHitboxActivator : MonoBehaviour
    {
        [SerializeField] private GameObject hitboxRoot;
        
        [Min(0.01f)] [SerializeField] private float activeSeconds = 0.1f;
        [Min(0f)] [SerializeField] private float cooldownSeconds = 0.15f;
        
        private float _cooldownUntilUnscaled = -1f;

        private void Awake()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
        }
        
        public bool CanAttack => UnityEngine.Time.unscaledTime >= _cooldownUntilUnscaled;

        /// <summary>
        /// 공격 요청 처리 : 쿨다운 체크 후 hitbox를 켰다 끈다.
        /// </summary>
        public void RequestAttack()
        {
            if (hitboxRoot == null) return;
            
            float now = UnityEngine.Time.unscaledTime;
            if (cooldownSeconds > 0f && now < _cooldownUntilUnscaled) return;
            
            _cooldownUntilUnscaled = now + cooldownSeconds;
            
            hitboxRoot.SetActive(true);
            CancelInvoke(nameof(DisableHitbox));
            Invoke(nameof(DisableHitbox), activeSeconds);
        }

        private void DisableHitbox()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
        }
    }
}