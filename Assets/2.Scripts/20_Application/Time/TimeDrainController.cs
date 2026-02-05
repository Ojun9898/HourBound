using System;
using UnityEngine;
using Hourbound.Domain.Time;

namespace Hourbound.Presentation.Time
{
    // 시간이 자동으로 감소하도록 만드는 컨트롤러
    // 초당 drainPerSecond 만큼 TimeResource에서 시간을 소모한다.
    public sealed class TimeDrainController : MonoBehaviour
    {
        [Header("의존성 주입")]
        [SerializeField] private TimeResourcePresenter timePresenter;
        
        [Header("감소 설정")]
        [Min(0f)] [SerializeField] private float drainPerSecond = 1f;
        
        [Header("옵션")]
        [SerializeField] private bool useUnscaledTime = false; // true면 타임스케일 0에서도 감소.
        
        [Header("스파이크 방지")]
        [Tooltip("씬 시작 직후 첫 프레임의 비정상 dt로 과도 소모되는 것을 방지")]
        [SerializeField] private bool ignoreFirstFrame = true;
        
        [Tooltip("프레임 dt 상한 (초). 0.05= 최대 50ms(20fps)로 제한")]
        [Min(0f)] [SerializeField] private float maxDeltaTime = 0.05f;
        
        private ITimeResource _time;
        private bool _skipOnce;

        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("TimeDrainController : TimeResourcePresenter가 연결되지 않았습니다.");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            _time = timePresenter.Time;

            if (_time == null)
            {
                Debug.LogError("TimeDrainController : Time이 아직 준비되지 않았습니다.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (_time == null) return;
            if (drainPerSecond <= 0) return;
            
            // 첫 프레임 dt 스파이크 방지
            if (_skipOnce)
            {
                _skipOnce = false;
                return;
            }
            
            float dt = useUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
            if (dt <= 0f) return;
            
            // dt 스파이크 방지(포커스 이동/씬 로드/에디터 등)
            if (maxDeltaTime > 0f && dt > maxDeltaTime)
                dt = maxDeltaTime;
            
            float amount = drainPerSecond * dt;
            
            // 소모 시도
            bool ok = _time.TrySpend(amount, new TimeSpendContext(TimeReason.AutoDrain, TimeSource.PerSecond));

            // 소모 실패시 0이 아니면 0으로 스냅
            if (!ok && _time.Current > 0f)
                _time.SetCurrent(0f, new TimeSetContext(TimeReason.AutoDrain));
        }
    }
}