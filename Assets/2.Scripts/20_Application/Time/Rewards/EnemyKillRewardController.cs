using System.Collections.Generic;
using UnityEngine;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Combat;
using Hourbound.Application.Rewards;

namespace Hourbound.Presentation.Time
{
    public sealed class EnemyKillRewardController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TimeResourcePresenter timePresenter;

        [Header("Base Reward")]
        [Min(0f)] [SerializeField] private float baseGainSeconds = 1.0f;
        
        [Header("Combo Weight")]
        [Min(0f)] [SerializeField] private float comboWindowSeconds = 3.0f;
        [Min(0f)] [SerializeField] private float comboStep = 0.2f;
        [Min(1f)] [SerializeField] private float comboMaxMultiplier = 2.0f;
        
        private float _lastKillUnscaled = -999f;
        private int _streak = 0;
        
        // 증복 보상/중복 구독 방지
        private readonly HashSet<int> _registered = new();
        private readonly HashSet<int> _rewarded = new();
        
        // 임시 : HUD 디버그용 프로퍼티
        public int Debug_Streak { get; private set; }
        public float Debug_ComboMultiplier { get; private set; }
        public float Debug_EnemyMultiplier { get; private set; }
        public float Debug_FinalMultiplier { get; private set; }
        public float Debug_LastGainSeconds { get; private set; }
        public string Debug_LastSourceId { get; private set; }
        public float Debug_LastKillUnscaledTime { get; private set; }

        public void Register(EnemyHealth enemy)
        {
            if (enemy == null) return;
            
            int id = enemy.GetInstanceID();
            if (!_registered.Add(id)) return;
            
            enemy.Died += OnEnemyDied;
        }

        public void UnRegister(EnemyHealth enemy)
        {
            if (enemy == null) return;
            
            int id = enemy.GetInstanceID();
            if (!_registered.Remove(id)) return;
            
            enemy.Died -= OnEnemyDied;
        }

        private void OnEnemyDied(EnemyHealth deadEnemy)
        {
            if (deadEnemy == null || timePresenter == null) return;
            
            int id = deadEnemy.GetInstanceID();
            if (_rewarded.Contains(id)) return;
            
            // 1) 콤보 멀티플라이어 계산 (일시정지 영향 줄이기 위해 unscaledTime 사용)
            float now = UnityEngine.Time.unscaledTime;
            if (now - _lastKillUnscaled <= comboWindowSeconds) _streak++;
            else _streak = 1;
            
            _lastKillUnscaled = now;
            
            float comboMul  = 1f + Mathf.Max(0, _streak - 1) * comboStep;
            comboMul = Mathf.Min(comboMul, comboMaxMultiplier);
            
            // 2) 적 티어(엘리트/보스) 멀티 플라이어 (없으면 1)
            float enemyMul = 1f;
            string sourceId = deadEnemy.gameObject.name;

            if (deadEnemy.TryGetComponent<IKillRewardMultiplier>(out var rewardInfo))
            {
                enemyMul = Mathf.Max(0f, rewardInfo.KillRewardMultiplier);
                if (!string.IsNullOrEmpty(rewardInfo.KillSourceId))
                    sourceId = rewardInfo.KillSourceId;
            }
            
            float finalMul = comboMul * enemyMul;
            float gain = baseGainSeconds * finalMul;
            
            var time = timePresenter.Time;
            time.Gain(gain, new TimeGainContext("적 처치", sourceId, finalMul));
            
            // 임시 : 디버그 HUD 용도
            Debug_Streak = _streak;
            Debug_ComboMultiplier = comboMul;
            Debug_EnemyMultiplier = enemyMul;
            Debug_FinalMultiplier = finalMul;
            Debug_LastGainSeconds = gain;
            Debug_LastSourceId = sourceId;
            Debug_LastKillUnscaledTime = UnityEngine.Time.unscaledTime;
        }
        
        // 임시 : 씬에 배치된 적을 자동 등록 하는 용도
        [Header("Temp (Scene enemies)")]
        [SerializeField] private EnemyHealth[] sceneEnemies;

        private void OnEnable()
        {
            if (sceneEnemies == null) return;
            foreach (var e in sceneEnemies) Register(e);
        }

        private void OnDisable()
        {
            if (sceneEnemies == null) return;
            foreach (var e in sceneEnemies) UnRegister(e);
        }
    }
}