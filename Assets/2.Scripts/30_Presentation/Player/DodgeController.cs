using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hourbound.Presentation.Player
{
    // 대시(회피) 컨트롤러
    // - 입력(Shift)로 대시 실행
    // - 일정 시간 무적(I-Frame) 부여
    // - 무적 구간 중 앞부분은 퍼펙트 윈도우로 취급
    public sealed class DodgeController : MonoBehaviour
    {
        [Header("대시 설정")]
        [Min(0f)] [SerializeField] private float dashDistance = 2.0f;
        [Min(0.01f)] [SerializeField] private float dashDuration = 0.12f;
        
        [Header("무적/퍼펙트윈도우")]
        [Min(0f)] [SerializeField] private float iframeDuration = 0.25f;
        [Min(0f)] [SerializeField] private float perfectWindowDuration = 0.12f;
        
        [Header("입력(디버그 용)")]
        [SerializeField] private Key dodgeKey = Key.LeftShift;
        
        [Header("옵션")]
        [SerializeField] private bool useRigidbody2D = true;
        
        public event Action DodgeStarted;
        public event Action PerfectWindowStarted;
        
        public bool IsInvincible { get; private set; }
        public bool IsPerfectWindow { get; private set; }
        
        private Rigidbody2D _rb;
        private float _invincibleUntilRealtime;
        private float _perfectUntilRealtime;

#if UNITY_EDITOR
        private InputAction _dodgeAction;
#endif

        private void Awake()
        {
            if (useRigidbody2D)
                _rb = GetComponent<Rigidbody2D>();

#if UNITY_EDITOR
            _dodgeAction = new InputAction("디버그_회피");
            _dodgeAction.AddBinding($"<Keyboard>/{dodgeKey.ToString().ToLower()}");
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (_dodgeAction != null)
            {
                _dodgeAction.performed += OnDodge;
                _dodgeAction.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (_dodgeAction != null)
            {
                _dodgeAction.performed -= OnDodge;
                _dodgeAction.Disable();
            }
#endif
        }

        private void Update()
        {
            // 타임스케일 영향을 받지 않도록 실시간 기준으로 처리
            float now = UnityEngine.Time.realtimeSinceStartup;
            IsInvincible = now < _invincibleUntilRealtime;
            IsPerfectWindow = now < _perfectUntilRealtime;
        }

#if UNITY_EDITOR
        private void OnDodge(InputAction.CallbackContext ctx)
        {
            StartDodge();
        }
#endif

        private void StartDodge()
        {
            // 방향은 입력(WASD/방향키)에서 즉석으로 추출
            // TODO : 추후 이동 입력 시스템과 연결
            Vector2 dir = ReadMoveDirection();
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector2.up; // 입력이 없으면 임시로 뒤로 설정
            
            dir.Normalize();
            
            // 이동(간단한 버전 : 즉시 이동)
            Vector2 delta = dir * dashDistance;
            if (useRigidbody2D && _rb != null)
                _rb.MovePosition(_rb.position + delta);
            else
                transform.position += (Vector3)delta;
            
            DodgeStarted?.Invoke();
            
            // 무적/퍼펙트 윈도우 시작
            float now = UnityEngine.Time.realtimeSinceStartup;
            _invincibleUntilRealtime = now + iframeDuration;
            _perfectUntilRealtime = now + Mathf.Min(perfectWindowDuration, iframeDuration);
            
            PerfectWindowStarted?.Invoke();
            
            Debug.Log($"회피 실행 : 무적 시간 = {iframeDuration:F2}s, 퍼펙트 닷지 = {perfectWindowDuration:F2}s", this);
        }

        private Vector2 ReadMoveDirection()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;
            
            float x = 0f;
            float y = 0f;
            
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += -1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= -1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += -1f;
            
            return new Vector2(x, y);
        }
    }
}
