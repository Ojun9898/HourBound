using UnityEngine;
using Hourbound.Presentation.Player;

namespace Hourbound.Presentation.Feedback.Dash
{
    /// <summary>
    /// DodgeController의 이벤트/상태에 맞춰 무적 VFX를 켜고 끈다.
    /// </summary>
    public sealed class DodgeInvincibleVfxBinder : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private DodgeController dodge;
        
        [Header("VFX")] 
        [SerializeField] private TrailRenderer[] trails;
        
        [Tooltip("대시 시작과 동시에 켤지")]
        [SerializeField] private bool enableOnDodgeStarted = true;
        
        [Tooltip("무적 종료까지 켤지")]
        [SerializeField] private bool keepUntilInvincibleEnds = true;
        
        private bool _active;

        private void Reset()
        {
            dodge = GetComponentInParent<DodgeController>();
        }

        private void Awake()
        {
            SetEnabled(false, clear: true);
        }

        private void OnEnable()
        {
            if (dodge == null) return;
            
            dodge.DodgeStarted += OnDodgeStarted;
            dodge.DodgeEnded += OnDodgeEnded;
        }

        private void OnDisable()
        {
            if (dodge == null) return;
            
            dodge.DodgeStarted -= OnDodgeStarted;
            dodge.DodgeEnded -= OnDodgeEnded;
        }

        private void Update()
        {
            if (!keepUntilInvincibleEnds) return;
            if (!_active) return;
            if (dodge == null) return;
            
            // 무적이 끝나면 자동 OFF
            if (!dodge.IsInvincible)
                SetEnabled(false, clear: false);
        }

        private void OnDodgeStarted()
        {
            if (!enableOnDodgeStarted) return;
            SetEnabled(true, clear: true);
        }

        private void OnDodgeEnded()
        {
            if (!keepUntilInvincibleEnds)
            {
                // 닷지가 끝나면 바로 OFF
                SetEnabled(false, clear: false);
            }
            // keepUntilInvincibleEnds면 Update에서 무적 끝날 때 OFF
        }

        private void SetEnabled(bool enabled, bool clear)
        {
            _active = enabled;
            
            if (trails == null) return;
            for (int i = 0; i < trails.Length; i++)
            {
                var t = trails[i];
                if (t == null) continue;
                
                if (clear) t.Clear();
                t.enabled = true;
                t.emitting = enabled;
            }
        }
    }
}