using System;
using UnityEngine;
using Hourbound.Presentation.Player;

namespace Hourbound.Presentation.Combat
{
    // 플레이어가 "피격 시도"를 받는 컴포넌트
    // - 무적이면 피해 대신 회피 판정
    // - 퍼펙트 윈도우 중이면 퍼펙트 회피 이벤트 발생
    public sealed class PlayerHitReceiver : MonoBehaviour
    {
        [Header("의존성 주입")]
        [SerializeField] private DodgeController dodge;
        
        public event Action Dodged; // 일반 회피
        public event Action PerfectDodged; // 퍼펙트 회피
        public event Action Damaged; // 실제 피격 (추후 체력/경직 연결)

        private void Awake()
        {
            if (dodge == null)
            {
                Debug.LogError("PlayerHitReceiver : DodgeController가 연결되지 않았습니다.");
                enabled = false;
            }
        }
        
        // 외부(적 공격 / 투사체 / 디버그)에서 호출하는 피격 시도 함수
        public void ReceiveHit(string source = "알수없음")
        {
            if (!enabled) return;

            if (dodge != null && dodge.IsInvincible)
            {
                if (dodge.IsPerfectWindow)
                {
                    PerfectDodged?.Invoke();
                    Debug.Log($"퍼펙트 회피 성공! (츨처 = {source})", this);
                }
                else
                {
                    Dodged?.Invoke();
                    Debug.Log($"회피  성공! (출처 = {source})", this);
                }
                
                return;
            }
            
            // 실제 피해 처리
            Damaged?.Invoke();
            Debug.Log($"피격! (출처 = {source})", this);
        }
    }
}