using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Time;

namespace HourBound.Presentation.Debugging
{
    // 디버그용 런 재시작 컨트롤러
    // R 키를 누르면 시간 / 타임스케일을 초기화함.
    // (씬 리로드 대신 루프 테스트를 하기 위한 초기화 기능)
    public sealed class DebugRunRestartController : MonoBehaviour
    {
        [Header("의존성 주입")] 
        [SerializeField] private TimeResourcePresenter timePresenter;

        [Header("초기화 설정")] 
        [Min(0.1f)] [SerializeField] private float resetMaxTime = 60f;
        [Min(0f)] [SerializeField] private float resetStartTime = 60f;
        [SerializeField] private bool resetTimeScale = true;

#if UNITY_EDITOR
        private InputAction _restartAction;
#endif

        private ITimeResource _time;

        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("DebugRunRestartController : TimeResourcePresenter가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            // R 키로 재시작
            _restartAction = new InputAction("디버그_재시작");
            _restartAction.AddBinding("<Keyboard>/r");
#endif
        }

        private void Start()
        {
            _time = timePresenter.Time;

            if (_time == null)
            {
                Debug.LogError("DebugRunRestartController : Time이 아직 준비되지 않았습니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (_restartAction != null)
            {
                _restartAction.performed += OnRestart;
                _restartAction.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (_restartAction != null)
            {
                _restartAction.performed -= OnRestart;
                _restartAction.Disable();
            }
#endif
        }

#if UNITY_EDITOR
        private void OnRestart(InputAction.CallbackContext ctx)
        {
            if (_time == null) return;
            
            // 패배 처리로 멈춘 타임스케일 복구
            if (resetTimeScale)
                Time.timeScale = 1f;
            
            // 최대치 / 현재치 초기화
            _time.SetMax(resetMaxTime, keepRatio: false, new TimeMaxChangedContext("디버그_재시작"));
            _time.SetCurrent(resetStartTime, new TimeSetContext("디버그_재시작"));

            Debug.Log("디버그 : 시간과 타임스케일을 초기화 했습니다.", this);
        }
#endif
    }
}