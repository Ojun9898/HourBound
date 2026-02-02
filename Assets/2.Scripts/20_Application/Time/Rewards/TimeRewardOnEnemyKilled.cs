using System;
using UnityEngine;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Combat;

namespace Hourbound.Presentation.Time
{
    // 적 처치 시 시간 보상을 지급하는 컴포넌트
    // 적의 Died 이벤트를 구독하고, TimeResource에 Gain을 호출한다.
    public sealed class TimeRewardOnEnemyKilled : MonoBehaviour
    {
        [Header("의존성 주입")] 
        [SerializeField] private TimeResourcePresenter timePresenter;

        [Header("대상 적")] 
        [SerializeField] private EnemyHealth targetEnemy;

        [Header("보상 설정")] 
        [Min(0f)] [SerializeField] private float timeRewardOnKill = 3f;

        private ITimeResource _time;

        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("TimeRewardOnEnemyKilled : TimeResourcePresenter가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

            if (targetEnemy == null)
            {
                Debug.LogError("TimeRewardOnEnemyKilled : 대상 적(EnemyHealth)이 연결되지 않았습니다.");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            _time = timePresenter.Time;

            if (_time == null)
            {
                Debug.LogError("TimeRewardOnEnemyKilled : Time이 아직 준비되지 않았습니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (targetEnemy != null)
                targetEnemy.Died += HandleEnemyDied;
        }

        private void OnDisable()
        {
            if (targetEnemy != null)
                targetEnemy.Died -= HandleEnemyDied;
        }

        private void HandleEnemyDied(EnemyHealth enemy)
        {
            if (_time == null) return;
            
            _time.Gain(timeRewardOnKill, new TimeGainContext("처치", enemy.name));
            Debug.Log($"처치 보상 : 시간 +{timeRewardOnKill} (대상 = {enemy.name}", this);
        }
    }
}