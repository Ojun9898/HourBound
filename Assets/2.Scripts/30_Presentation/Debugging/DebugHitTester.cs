using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Combat;

namespace HourBound.Presentation.Debugging
{
    // 디버그용 피격 테스트 도구
    // H 키를 누르면 플레이어에게 피격 시도를 보낸다.
    public sealed class DebugHitTester : MonoBehaviour
    {
        [SerializeField] private PlayerHitReceiver target;
        [SerializeField] private Key hitKey = Key.H;

#if UNITY_EDITOR
        private InputAction _hitAction;
#endif

        private void Awake()
        {
            if (target == null)
            {
                Debug.LogError("DebugHitTester : PlayerHitReceiver가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            _hitAction = new InputAction("디버그_피격");
            _hitAction.AddBinding($"<Keyboard>/{hitKey.ToString().ToLower()}");
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (_hitAction != null)
            {
                _hitAction.performed += OnHit;
                _hitAction.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (_hitAction != null)
            {
                _hitAction.performed -= OnHit;
                _hitAction.Disable();
            }
#endif
        }

#if UNITY_EDITOR
        private void OnHit(InputAction.CallbackContext ctx)
        {
            target.ReceiveHit("디버그(H)");
            Debug.Log("플레이어가 피해를 입었습니다!");
        }
#endif
    }
}