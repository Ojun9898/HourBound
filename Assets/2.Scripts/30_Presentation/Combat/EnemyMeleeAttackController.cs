using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Combat.Hitbox;

namespace Hourbound.Presentation.Combat
{
    // 적 근접 공격을 실행하는 테스트용 컨트롤러
    // J키를 누르면 히트박스를 잠깐 활성화한다.
    public sealed class EnemyMeleeAttackController : MonoBehaviour
    {
        [Header("히트박스")]
        [SerializeField] private MeleeHitBox hitbox;
        
        [Header("공격 시간(히트박스 활성 시간)")]
        [Min(0.01f)] [SerializeField] private float activeSeconds = 0.12f;
        
        [Header("입력(디버그용)")]
        [SerializeField] private Key attackKey = Key.J;

#if UNITY_EDITOR
        private InputAction _attackAction;
#endif
        
        private float _endRealtime;
        private bool _attacking;

        private void Awake()
        {
            if (hitbox == null)
            {
                Debug.LogError("EnemyMeleeAttackController : MeleeHitbox가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            
            // 시작 시 꺼둠
            hitbox.gameObject.SetActive(false);

#if UNITY_EDITOR
            _attackAction = new InputAction("디버그_직접공격");
            _attackAction.AddBinding($"<Keyboard>/{attackKey.ToString().ToLower()}");
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            _attackAction.performed += OnAttack;
            _attackAction.Enable();
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            _attackAction.performed -= OnAttack;
            _attackAction.Disable();
#endif
        }

        private void Update()
        {
            if (!_attacking) return;

            if (UnityEngine.Time.realtimeSinceStartup >= _endRealtime)
            {
                _attacking = false;
                hitbox.End();
            }
        }

#if UNITY_EDITOR
        private void OnAttack(InputAction.CallbackContext ctx)
        {
            // 공격 시작
            _attacking = true;
            _endRealtime = UnityEngine.Time.realtimeSinceStartup + activeSeconds;
            hitbox.Begin();
            
            Debug.Log("적 근접 공격 실행(히트박스 ON)", this);
        }
#endif
    }
}