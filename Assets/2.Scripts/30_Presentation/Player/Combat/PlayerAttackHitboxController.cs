using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hourbound.Presentation.Player.Combat
{
    /// <summary>
    /// InputSystem의 Attack 액션을 받아, 근접 히트박스를 짧은 시간 활성화하는 컨트롤러.
    /// - 임시 전투 파이프라인 (Attack -> Hitbox -> EnemyHealth.TakeDamage) 검증
    /// TODO : 애니메이션 이벤트로 교체/연결하기 쉬운 구조로 변경.
    /// </summary>
    public sealed class PlayerAttackHitboxController : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("InputActions의 Attack(button) 액션을 바인딩")]
        [SerializeField] private InputActionReference attackAction;
        
        [Header("Hitbox")]
        [Tooltip("Trigger Collider가 달린 히트박스 루트 오브젝트")]
        [SerializeField] private GameObject hitboxRoot;
        
        [Min(0.01f)] [SerializeField] private float activeSeconds = 0.1f;
        
        [Header("Optional")]
        [Min(0f)] [SerializeField] private float cooldownSeconds = 0.15f;
        
        private float _cooldownUntilUnscaled = -1f;

        private void Awake()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (attackAction?.action == null) return;
            
            attackAction.action.Enable();
            attackAction.action.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            if (attackAction?.action == null) return;
            
            attackAction.action.performed -= OnAttackPerformed;
            
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
        }

        private void OnAttackPerformed(InputAction.CallbackContext _)
        {
            if (hitboxRoot == null) return;
            
            float now = UnityEngine.Time.unscaledTime;
            
            if (cooldownSeconds > 0f && now < _cooldownUntilUnscaled) return;
            
            _cooldownUntilUnscaled = now + cooldownSeconds;
            
            // 히트박스 켜기
            hitboxRoot.SetActive(true);
            
            // N초 후 끄기
            CancelInvoke(nameof(DisableHitbox));
            Invoke(nameof(DisableHitbox), activeSeconds);
        }

        private void DisableHitbox()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
        }
    }
}