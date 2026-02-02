using System;
using UnityEngine;
using Hourbound.Presentation.Combat;

namespace Hourbound.Presentation.Feedback
{
    // 피격 / 회피 / 퍼펙트 닷지 이벤트에 맞춰 VFX / SFX / 히트스톱 실행
    public sealed class PlayerCombatFeedback : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private PlayerHitReceiver hitReceiver;
        [SerializeField] private Animator playerAnimator;
        
        [Header("서비스")]
        [SerializeField] private HitStopService hitStop;
        [SerializeField] private SfxService sfx;
        [SerializeField] private VfxService vfx;
        
        [Header("리소스")]
        [SerializeField] private AudioClip damagedSfx;
        [SerializeField] private AudioClip dodgeSfx;
        [SerializeField] private AudioClip perfectDodgeSfx;
        
        [SerializeField] private GameObject damagedVfxPrefab;
        [SerializeField] private GameObject perfectVfxPrefab;
        
        [Header("히트스톱")]
        [Min(0f)] [SerializeField] private float damagedStopSeconds = 0.05f;
        [Min(0f)] [SerializeField] private float perfectStopSeconds = 0.06f;

        private void Awake()
        {
            if (hitReceiver == null)
            {
                Debug.LogError("PlayerCombatFeedback : PlayerHitReceiver가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            
            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();

            if (hitStop == null || sfx == null || vfx == null)
            {
                Debug.LogError("PlayerCombatFeedback : 피드백 서비스가 연결되지 않았습니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            hitReceiver.Damaged += OnDamaged;
            hitReceiver.Dodged += OnDodged;
            hitReceiver.PerfectDodged += OnPerfectDodged;
        }

        private void OnDisable()
        {
            hitReceiver.Damaged -= OnDamaged;
            hitReceiver.Dodged -= OnDodged;
            hitReceiver.PerfectDodged -= OnPerfectDodged;
        }

        private void OnDamaged()
        {
            if (sfx != null) sfx.Play(damagedSfx);
            if (vfx != null) vfx.Spawn(damagedVfxPrefab, hitReceiver.transform.position, Quaternion.identity);
            if (hitStop != null) hitStop.StopAnimator(playerAnimator, damagedStopSeconds);
        }

        private void OnDodged()
        {
            if (sfx != null) sfx.Play(dodgeSfx);
        }

        private void OnPerfectDodged()
        {
            if (sfx != null) sfx.Play(perfectDodgeSfx);
            if (vfx != null) vfx.Spawn(perfectVfxPrefab, hitReceiver.transform.position, Quaternion.identity);
            if (hitStop != null) hitStop.StopAnimator(playerAnimator, perfectStopSeconds);
        }
    }
}