using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Time;

namespace Hourbound.Presentation.Debugging
{
    public sealed class DebugTimeCheatController : MonoBehaviour
    {
        [Header("의존성 주입")] 
        [SerializeField] private TimeResourcePresenter timePresenter;

        [Header("디버그 설정")] 
        [SerializeField] private float spendAmount = 5f;
        [SerializeField] private float gainAmount = 5f;

        private ITimeResource _time;

        private InputAction _debugSpend;
        private InputAction _debugGain;

        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("DebugTimeCheatController : TimeResourcePresenter가 연결되지 않았습니다.");
                enabled = false;
                return;
            }
            
#if UNITY_EDITOR
            // 디버그 액션 생성(키 바인딩)
            _debugSpend = new InputAction("디버그_소모");
            _debugSpend.AddBinding("<Keyboard>/a");
            _debugSpend.AddBinding("<Keyboard>/numpadMinus");

            _debugGain = new InputAction("디버그_회복");
            _debugGain.AddBinding("<Keyboard>/s");
            _debugGain.AddBinding("<Keyboard>/numpadPlus");
#endif
        }

        private void Start()
        {
            _time = timePresenter.Time;
            
            if (timePresenter == null)
            {
                Debug.LogError("DebugTimeCheatController : TimeResourcePresenter가 연결되지 않았습니다.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            // Debug.Log("DebugTimeCheatController OnEnable 호출됨", this);
            
            if (_debugSpend != null)
            {
                _debugSpend.performed += OnSpend;
                _debugSpend.Enable();
            }

            if (_debugGain != null)
            {
                _debugGain.performed += OnGain;
                _debugGain.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (_debugSpend != null)
            {
                _debugSpend.performed -= OnSpend;
                _debugSpend.Disable();
            }

            if (_debugGain != null)
            {
                _debugGain.performed -= OnGain;
                _debugGain.Disable();
            }
#endif
        }

        private void OnSpend(InputAction.CallbackContext ctx)
        {
            if (_time == null)
            {
                Debug.LogWarning("DebugTimeCheatController : Time이 null이라 실행하지 못했습니다.");
                return;
            }
            
            bool ok = _time.TrySpend(spendAmount, new TimeSpendContext("디버그", "액션 : 소모"));
            Debug.Log($"시간 소모 입력! 성공 = {ok}, 현재 {_time.Current}/{_time.Max}", this);
        }

        private void OnGain(InputAction.CallbackContext ctx)
        {
            if (_time == null)
            {
                Debug.LogWarning("DebugTimeCheatController : Time이 null이라 실행하지 못했습니다.");
                return;
            }
            
            _time.Gain(gainAmount, new TimeGainContext("디버그", "액션 : 회복"));
            Debug.Log($"시간 회복 성공!, 현재 {_time.Current}/{_time.Max}", this);
        }
    }
}