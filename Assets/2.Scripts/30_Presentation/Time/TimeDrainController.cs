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
        
        private ITimeResource _time;

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
            
            float dt = useUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
            if (dt <= 0f) return;
            
            float amount = drainPerSecond * dt;
            
            // 자동 감소는 실패하는 경우(시간이 이미 0인 경우) 조용히 넘어가도 됨
            _time.TrySpend(amount, new TimeSpendContext("자동감소", "초당감소"));
        }
    }
}

