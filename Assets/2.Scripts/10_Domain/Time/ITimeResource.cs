using System;

namespace Hourbound.Domain.Time
{
    public interface ITimeResource
    {
        float Current { get; }
        float Max { get; }

        event Action<TimeChangedArgs> Changed;
        event Action<TimeDepletedArgs> Depleted;

        bool CanSpend(float amount);

        // 시간 소모 시도: 성공하면 true, 부족하면 false
        bool TrySpend(float amount, TimeSpendContext context);

        // 시간 회복: 음수는 허용하지 않음
        void Gain(float amount, TimeGainContext context);

        // 최대 시간 변경: 유물/레벨업 등으로 Max가 바뀔 때 사용
        // keepRatio가 true면 현재 비율(Current/Max)을 유지한 채로 Max를 바꿈
        void SetMax(float newMax, bool keepRatio, TimeMaxChangedContext context);

        // 현재 시간 강제 설정: 런 시작/리셋/디버그용
        void SetCurrent(float newCurrent, TimeSetContext context);
    }
}