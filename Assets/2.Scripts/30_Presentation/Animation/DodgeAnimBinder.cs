using UnityEngine;
using Hourbound.Presentation.Player;

namespace Hourbound.Presentation.Animation
{
    public sealed class DodgeAnimBinder : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private DodgeController dodge;
        [SerializeField] private Animator animator;
        
        [Header("Animator Params")]
        [SerializeField] private string dodgeTrigger = "Dodge";
        [SerializeField] private string perfectBool = "PerfectWindow"; // 선택사항
        
        private int _dodgeTriggerHash;
        private int _perfectBoolHash;

        private void Reset()
        {
            dodge = GetComponentInParent<DodgeController>();
            animator = GetComponentInParent<Animator>();
        }

        private void Awake()
        {
            _dodgeTriggerHash = Animator.StringToHash(dodgeTrigger);
            _perfectBoolHash = Animator.StringToHash(perfectBool);
        }

        private void OnEnable()
        {
            if (dodge == null || animator == null) return;
            
            dodge.DodgeStarted += OnDodgeStarted;
            dodge.DodgeEnded += OnDodgeEnded;
            dodge.PerfectWindowStarted += OnPerfectStarted;
            dodge.PerfectWindowEnded += OnPerfectEnded;
        }

        private void OnDisable()
        {
            if (dodge == null || animator == null) return;
            
            dodge.DodgeStarted -= OnDodgeStarted;
            dodge.DodgeEnded -= OnDodgeEnded;
            dodge.PerfectWindowStarted -= OnPerfectStarted;
            dodge.PerfectWindowEnded -= OnPerfectEnded;
        }

        private void OnDodgeStarted()
        {
            animator.ResetTrigger(_dodgeTriggerHash);
            animator.SetTrigger(_dodgeTriggerHash);
        }

        private void OnDodgeEnded()
        {
            // 보통 Locomotion으로 지연 복귀(Exit Time)면 아무것도 안 해도 됨.
            // 강제 복귀를 원하면 여기서 다른 파라미터를 건드려도 됨.
        }

        private void OnPerfectStarted()
        {
            // 선택 : 퍼펙트 윈도우 동안 강조(레이어/머티리얼/포스트프로세싱도 여기서)
            if (!string.IsNullOrEmpty(perfectBool))
                animator.SetBool(_perfectBoolHash, true);
        }

        private void OnPerfectEnded()
        {
            if (!string.IsNullOrEmpty(perfectBool))
                animator.SetBool(_perfectBoolHash, false);
        }
    }
}

