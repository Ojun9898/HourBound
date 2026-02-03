using System;
using UnityEngine;
using Hourbound.Presentation.Player;

namespace Hourbound.Presentation.Combat
{
    /// <summary>
    /// 플레이어가 "피격 시도"를 받는 컴포넌트 (루트에 붙이는 것을 권장)
    /// - 무적이면 Dodged / PerfectDodged
    /// - 무적이 아니면 Damaged
    /// - 실제 체력(시간) 감소, VFX/SFX, 경직 등은 이벤트 구독자가 처리
    /// </summary>
    public sealed class PlayerHitReceiver : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DodgeController dodge;
        
        [Header("Anti-Spam")]
        [Tooltip("짧은 시간 내 피격 시도 스팸 방지(트리거 중복/여러 콜라이더 대비)")]
        [SerializeField] private float minHitInterval = 0.08f;
        
        [Header("Debug")]
        [SerializeField] private bool logReceiveHit = true;

        private float _nextAcceptTime;
        
        // 기존 이벤트 유지
        public event Action Dodged;
        public event Action PerfectDodged;
        public event Action Damaged;

        // (선택) 나중에 출처가 필요해지면 구독자가 쓸 수 있게 source 포함 이벤트도 제공
        public event Action<GameObject> DodgedFrom;
        public event Action<GameObject> PerfectDodgedFrom;
        public event Action<GameObject> DamagedFrom;

        private void Reset()
        {
            dodge = GetComponentInParent<DodgeController>();
        }

        private void Awake()
        {
            if (dodge == null)
                dodge = GetComponentInParent<DodgeController>();

            if (dodge == null)
                Debug.LogError("PlayerHitReceiver: DodgeController를 찾지 못했습니다.", this);
        }

        /// <summary>
        /// 외부(적 히트박스/투사체/디버그)에서 호출하는 피격 시도 함수
        /// </summary>
        public void ReceiveHit(GameObject source)
        {
            // 중복 피격 시도 방지
            if (minHitInterval > 0f && UnityEngine.Time.time < _nextAcceptTime)
                return;
            _nextAcceptTime = UnityEngine.Time.time + minHitInterval;
            
            if (logReceiveHit)
                Debug.Log($"[RECEIVE HIT] source = {source?.name}", this);
            
            // 무적이면 회피
            if (dodge != null && dodge.IsInvincible)
            {
                if (dodge.IsPerfectWindow)
                {
                    PerfectDodged?.Invoke();
                    PerfectDodgedFrom?.Invoke(source);
                    if (logReceiveHit) Debug.Log("[RECEIVE HIT] -> PerfectDodged", this);
                }
                else
                {
                    Dodged?.Invoke();
                    DodgedFrom?.Invoke(source);
                    if (logReceiveHit) Debug.Log("[RECEIVE HIT] -> Dodged", this);
                }
                return;
            }

            // 실제 피격
            Damaged?.Invoke();
            DamagedFrom?.Invoke(source);
            if (logReceiveHit) Debug.Log("[RECEIVE HIT] -> Damaged", this);
        }
    }
}