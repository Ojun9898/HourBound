using System;
using UnityEngine;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Combat;
using UnityEngine.Timeline;

namespace Hourbound.Presentation.Time
{
    // 퍼펙트 회피 성공 후 시간 보상을 지급하는 컴포넌트 
    public sealed class TimeRewardOnPerfectDodge : MonoBehaviour
    {
        [Header("의존성 주입")]
        [SerializeField] private TimeResourcePresenter timePresenter;
        [SerializeField] private PlayerHitReceiver playerHit;
        
        [Header("보상 설정")]
        [Min(0f)] [SerializeField] private float timeReward = 2.5f;
        
        private ITimeResource _time;

        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("TimeRewardOnPerfectDodge : TimeResourcePresenter가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

            if (playerHit == null)
            {
                Debug.LogError("TimeRewardOnPerfectDodge : PlayerHitReceiver가 연결되지 않았습니다.");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            _time = timePresenter.Time;
            if (_time == null)
            {
                Debug.LogError("TimeRewardOnPerfectDodge : Time이 아직 준비되지 않았습니다.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (playerHit != null)
                playerHit.PerfectDodged += HandlePerfectDodged;
        }

        private void OnDisable()
        {
            if (playerHit != null)
                playerHit.PerfectDodged -= HandlePerfectDodged;
        }

        private void HandlePerfectDodged()
        {
            if (_time == null) return;
            
            _time.Gain(timeReward, new TimeGainContext("퍼펙트회피", "보상"));
            Debug.Log($"퍼펙트 회피 보상 : 시간 + {timeReward}", this);
        }
    }
}