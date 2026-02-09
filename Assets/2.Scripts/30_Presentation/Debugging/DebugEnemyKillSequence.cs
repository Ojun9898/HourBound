using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Presentation.Combat;

namespace Hourbound.Presentation.Debugging
{
    /// <summary>
    /// 적 처치 보상( + 콤보 시스템 ) 테스트 용 스크립트
    /// 지정한 키 (default : K) 를 누르면 적에게 데미지를 가함. 
    /// </summary>
    public sealed class DebugEnemyKillSequence_InputRef : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference killAction; // 버튼 액션 연결

        [Header("Targets (in order)")]
        [SerializeField] private EnemyHealth[] enemies;

        [Header("Damage")]
        [SerializeField] private int killDamage = 9999;

        [Header("Options")]
        [SerializeField] private bool loop = false;
        [SerializeField] private bool skipNulls = true;

        private int _index = 0;

        private void OnEnable()
        {
            if (killAction?.action == null) return;

            killAction.action.Enable();
            killAction.action.performed += OnKillPerformed;
        }

        private void OnDisable()
        {
            if (killAction?.action == null) return;

            killAction.action.performed -= OnKillPerformed;
            // 주의: 다른 곳(예: PlayerInput)이 같은 액션을 관리한다면 Disable을 여기서 하지 않는 게 안전
            // killAction.action.Disable();
        }

        private void OnKillPerformed(InputAction.CallbackContext ctx)
        {
            KillNext();
        }

        private void KillNext()
        {
            if (enemies == null || enemies.Length == 0) return;
            if (!loop && _index >= enemies.Length) return;

            int tries = 0;
            while (tries < enemies.Length)
            {
                var e = enemies[_index];
                int current = _index;

                _index++;
                if (_index >= enemies.Length)
                    _index = loop ? 0 : enemies.Length;

                if (e != null)
                {
                    e.TakeDamage(killDamage);
                    Debug.Log($"[DEBUG] KillSequence: index={current}, enemy={e.name}");
                    return;
                }

                if (!skipNulls) return;

                if (!loop && _index >= enemies.Length) return;
                tries++;
            }
        }
    }
}