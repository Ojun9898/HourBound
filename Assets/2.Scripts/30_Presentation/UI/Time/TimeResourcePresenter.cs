using System;
using UnityEngine;
using Hourbound.Domain.Time;

namespace Hourbound.Presentation.Time
{
    // Unity와 도메인(TimeResource)을 연결하는 바인딩 컴포넌트
    // 씬에서 TimeResource를 생성하고, 다른 컴포넌트가 참조할 수 있게 제공한다.
    // 전역 싱글톤을 강제하지 않고, 인트펙터 참조 / 주입 방식으로 연결하는 것을 권장
    [DefaultExecutionOrder(-100)]
    public sealed class TimeResourcePresenter : MonoBehaviour
    {
        [Header("초기 설정")] 
        [Min(0.1f)] [SerializeField] private float maxTime = 60f;
        [Min(0f)] [SerializeField] private float startTime = 60f;
        
        // 외부에서 접근할 때는 인터페이스로 제공
        public ITimeResource Time { get; private set; }

        private void Awake()
        {
            // 도메인 객체 생성
            Time = new TimeResource(maxTime, startTime);
            Debug.Log($"[TimeCreated] current={Time.Current} max={Time.Max}", this);
        }
    }
}