using System;
using System.Collections.Generic;
using UnityEngine;
using Hourbound.Presentation.Combat;
using Hourbound.Presentation.Combat.Hitbox;

namespace Hourbound.Presentation.Combat.Hitbox
{
    /// <summary>
    /// 적 근접 히트박스
    /// - 기본은 비활성(콜라이더 off)
    /// - ActivateFor로 공격 윈도우 동안만 활성화
    /// - 같은 대상에 대한 중복 히트 방지(윈도우 내 1회 or 쿨다운)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MeleeHitBox : MonoBehaviour
    {
        public enum HitRepeatMode
        {
            OncePerWindow, // 윈도우 동안 대상당 1회만
            CooldownPerTarget // 대상별 쿨다운
        }
        
        [Header("Refs")]
        [Tooltip("피격 시도 전달 대상(보통 PlayerHitReceiver). 비우면 other에서 탐색.")]
        [SerializeField] private PlayerHitReceiver explicitReceiver;
        
        [Header("Repeat")]
        [SerializeField] private HitRepeatMode repeatMode = HitRepeatMode.OncePerWindow;
        
        [Tooltip("CooldownPerTarget일 때만 사용(초)")]
        [Min(0f)] [SerializeField] private float hitCooldown = 0.25f;
        
        [Header("Debug")]
        [SerializeField] private bool log;
        
        private Collider _col;
        private bool _active;
        
        // OncePerWindow용 : 대상 인스턴스ID 기록
        private readonly HashSet<int> _hitSet = new HashSet<int>();
        
        // CooldownPerTarget용 : 대상 인스턴스ID -> 다음 히트 가능 시간
        private readonly Dictionary<int, float> _nextHitAt = new Dictionary<int, float>();

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _col.isTrigger = true;
            
            // 기본 OFF
            SetActiveInternal(false, clearMemory: true);
        }

        /// <summary>
        /// 공격 윈도우 시작
        /// </summary>
        public void Activate()
        {
            SetActiveInternal(true, clearMemory: true);
        }

        /// <summary>
        /// 공격 윈도우 종료
        /// </summary>
        public void Deactivate()
        {
            SetActiveInternal(false, clearMemory: false);
        }

        /// <summary>
        /// duration 동안만 활성화
        /// </summary>
        /// <param name="duration"></param>
        public void ActiveFor(float duration)
        {
            Activate();
            if (duration > 0f)
                Invoke(nameof(Deactivate), duration);
        }

        private void SetActiveInternal(bool enabled, bool clearMemory)
        {
            _active = enabled;
            
            if (_col != null) _col.enabled = enabled;

            if (clearMemory)
            {
                _hitSet.Clear();
                _nextHitAt.Clear();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_active) return;
            
            // 자기 자신/같은 루트 무시
            if (other.transform.root == transform.root)
                return;
            
            // PlayerHitReceiver 찾기
            PlayerHitReceiver receiver = explicitReceiver;
            
            if (receiver == null)
                receiver = other.GetComponentInParent<PlayerHitReceiver>();
            
            if (receiver == null) return;
            
            int id = receiver.gameObject.GetInstanceID();

            if (repeatMode == HitRepeatMode.OncePerWindow)
            {
                if (_hitSet.Contains(id)) return;
                _hitSet.Add(id);
            }

            else // CooldownPerTarget
            {
                float now = UnityEngine.Time.timeSinceLevelLoad;
                if (_nextHitAt.TryGetValue(id, out float t) && now < t) return;
                _nextHitAt[id] = now + hitCooldown;
            }
            
            if (log)
                Debug.Log($"[MeleeHitBox] hit -> {receiver.name} by {name}", this);
            
            receiver.ReceiveHit(gameObject);
        }
    }
}