using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Combat;

namespace Hourbound.Presentation.Debugging
{
    // 디버그용 데미지 입력 컨트롤러
    // D 키를 누르면 대상 적에게 데미지를 준다.
    public sealed class DebugDamageDealer : MonoBehaviour
    {
        [SerializeField] private EnemyHealth targetEnemy;
        [Min(1)] [SerializeField] private int damage = 1;

#if UNITY_EDITOR
        private InputAction _dealDamage;
#endif

        private void Awake()
        {
            if (targetEnemy == null)
            {
                Debug.LogError("DebugDamageDealer : 대상 적(EnemyHealth)이 연결되지 않았습니다.");
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            _dealDamage = new InputAction("디버그_데미지");
            _dealDamage.AddBinding("<Keyboard>/d");
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (_dealDamage != null)
            {
                _dealDamage.performed += OnDealDamage;
                _dealDamage.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            _dealDamage.performed -= OnDealDamage;
            _dealDamage.Disable();
#endif
        }

#if UNITY_EDITOR
        private void OnDealDamage(InputAction.CallbackContext ctx)
        {
            targetEnemy.TakeDamage(damage);
            Debug.Log($"적 {targetEnemy.name}에게 {damage} 데미지를 가했습니다.", this);
        }
#endif
    }
}