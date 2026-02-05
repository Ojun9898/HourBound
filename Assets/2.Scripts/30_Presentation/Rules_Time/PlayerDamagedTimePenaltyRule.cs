using System;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Combat;
using Hourbound.Presentation.Time;
using UnityEngine;

namespace Hourbound.Presentation.Rules.Time
{
    /// <summary>
    ///  플레이어가 Damage를 받으면 시간 자원을 - 한다.
    /// - Domain TimeResource.TrySpend 사용
    /// - TimeResourcePresenter가 Domain 객체를 생성/보관하므로 Presenter를 참조한다.
    /// </summary>
    [DefaultExecutionOrder(-90)] // TimeResourcePresenter(-100) 이후 실행되게
    public sealed class PlayerDamagedTimePenaltyRule : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerHitReceiver receiver;
        [SerializeField] private TimeResourcePresenter timePresenter;

        [Header("Penalty")]
        [Min(0f)] [SerializeField] private float timePenalty = 3f;
        
        [Header("Debug")]
        [SerializeField] private bool log = false;

        private ITimeResource _time;

        private void Reset()
        {
            if (receiver == null) receiver = FindFirstObjectByType<PlayerHitReceiver>();
            if (timePresenter == null) timePresenter = FindFirstObjectByType<TimeResourcePresenter>();
        }

        private void Awake()
        {
            if (receiver == null)
            {
                Debug.LogError("PlayerDamagedTimePenaltyRule : PlayerHitReceiver가 비어있습니다.", this);
                enabled = false;
                return;
            }

            if (timePresenter == null)
            {
                Debug.LogError("PlayerDamagedTimePenaltyRule : TimeResourcePresenter가 비어있습니다.", this);
                enabled = false;
                return;
            }
            
            _time = timePresenter.Time;
            if (_time == null)
            {
                Debug.LogError("PlayerDamagedTimePenaltyRule : timePresenter.Time이 null입니다", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (receiver != null)
                receiver.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            if (receiver != null)
                receiver.Damaged -= OnDamaged;
        }

        private void OnDamaged()
        {
            // 규칙 표준 Context
            var ctx = new TimeSpendContext(TimeReason.Hit, TimeSource.Player);
            
            Debug.Log($"[PenaltyBefore] current={_time.Current}", this);
            bool ok = _time.TrySpend(timePenalty, ctx);
            Debug.Log($"[PenaltyAfter] current={_time.Current}", this);
            
            if (!ok && _time.Current > 0f)
                _time.SetCurrent(0f, new TimeSetContext(TimeReason.Hit));
            
            if (log)
                Debug.Log($"[TimePenalty] {TimeReason.Hit}/{TimeSource.Player} - {timePenalty} ok = {ok} current = {_time.Current}/{_time.Max}", this);
        }
    }
}