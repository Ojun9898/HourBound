using System;
using System.Collections;
using UnityEngine;
using Hourbound.Presentation.Player.Motion;
using Unity.VisualScripting;

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
        
        [Header("모터")]
        [SerializeField] private MonoBehaviour motorBehaviour;
        
        public event Action DodgeStarted;
        public event Action DodgeEnded;
        public event Action PerfectWindowStarted;
        public event Action PerfectWindowEnded;
        
        public bool IsInvincible => UnityEngine.Time.realtimeSinceStartup < _invincibleUntilRealtime;
        public bool IsPerfectWindow => UnityEngine.Time.realtimeSinceStartup < _perfectUntilRealtime;
        
        private IDodgeMotor _motor;
        private float _invincibleUntilRealtime;
        private float _perfectUntilRealtime;
        
        private bool _wasPerfect; // ended 이벤트용

        private void Awake()
        {
            _motor = motorBehaviour as IDodgeMotor;
            if (_motor == null)
                Debug.LogError("DodgeController : motorBehaviour에 IDodgeMotor 구현체를 할당해야 합니다.");
        }

        private void Update()
        {
            // PerfectWindowEnded는 "이전 프레임 true -> 이번 프레임 false"에서만 발생
            bool nowPerfect = IsPerfectWindow;
            if (_wasPerfect && !nowPerfect)
                PerfectWindowEnded?.Invoke();

            _wasPerfect = nowPerfect;
        }

        /// <summary>
        /// 외부(입력/AI/테스트)에서 호출하는 대시 요청
        /// moveInput : (x=좌우, y=전후)
        /// </summary>
        public void RequestDodge(Vector2 moveInput)
        {
            if (_motor == null) return;
            if (_motor.IsDodging) return;
            
            // 방향이 없으면 "전방"으로
            Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector3.forward;
            
            dir.Normalize();
            
            // 타이머 먼저 세팅 (이벤트 구독자가 즉시 읽어도 true)ㅣ
            float now = UnityEngine.Time.realtimeSinceStartup;
            _invincibleUntilRealtime = now + iframeDuration;
            _perfectUntilRealtime = now + Mathf.Min(perfectWindowDuration, iframeDuration);
            
            // 이벤트 발행
            DodgeStarted?.Invoke();
            PerfectWindowStarted?.Invoke();
            _wasPerfect = true;
            
            // 이동 실행
            _motor.Dodge(dir, dashDistance, dashDuration);
            
            // 대시 종료 이벤트가 끝났는지 폴링하는 간단한 방식으로 처리(필요하면 코루틴으로 더 깔끔하게)
            StartCoroutine(CoWaitDodgeEnd());
        }

        private IEnumerator CoWaitDodgeEnd()
        {
            // 모터 종료까지 대기
            while (_motor != null && _motor.IsDodging)
                yield return null;
            
            DodgeEnded?.Invoke();
        }
    }
}
