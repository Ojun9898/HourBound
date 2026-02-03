using System;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Combat;
using Hourbound.Presentation.Time;
using UnityEngine;

namespace HourBound.Presentation.Rules.Time
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
        [SerializeField]
        private PlayerHitReceiver receiver;
        [SerializeField]
        private TimeResourcePresenter timePresenter;

        [Header("Penalty")]
        [Min(0f)]
        [SerializeField]
        private float timePenalty = 3f;

        [Header("Debug")]
        [SerializeField]
        private bool log = false;

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
                Debug.LogError("PlayerDamagedTimePenaltyRule : PlayerHitReceiver가 비어있습니다.");
                enabled = false;
                return;
            }

            if (timePresenter == null)
            {
                
            }
        }
    }
}