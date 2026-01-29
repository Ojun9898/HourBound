using System;
using TMPro;
using UnityEngine;

namespace Hourbound.Domain.Time
{
    // 순수 도메인 로직 클래스
    // Unity(엔진) 의존 없이 시간 자원만 관리한다
    // UI 갱신, 게임오버 처리, 사운드/이펙트 등은 이벤트 구독자가 담당한다
    public sealed class TimeResource : ITimeResource
    {
        public float Current { get; private set; }
        public float Max { get; private set; }

        public event Action<TimeChangedArgs> Changed;
        public event Action<TimeDepletedArgs> Depleted;
        
        // 고갈 이벤트가 중복으로 발생하지 않게 막기 위한 플래그
        private bool _isDepletedRaised;

        public TimeResource(float max, float startCurrent)
        {
            if (max <= 0f) throw new ArgumentOutOfRangeException(nameof(max), "최대 시간(Max)은 0보다 커야 합니다.");
            
            Max = max;
            Current = Mathf.Clamp(startCurrent, 0f, Max);
            
            // 시작부터 0이면 이미 고갈 상태로 간주
            _isDepletedRaised = Current <= 0f;
        }

        public bool CanSpend(float amount)
        {
            // 0 이하 소모는 항상 가능으로 취급
            if (amount <= 0) return true;
            return Current >= amount;
        }
        
        public bool TrySpend(float amount, TimeSpendContext context)
        {
            if (amount < 0f) 
                throw new ArgumentOutOfRangeException(nameof(amount), "소모량은 음수가 될 수 없습니다.");

            if (amount == 0f)
                return true;
            
            // 시간이 부족하면 실패
            if (!CanSpend(amount))
                return false;

            float before = Current;
            Current = Mathf.Clamp(Current - amount, 0f, Max);
            
            // 시간 변화 이벤트 발생
            RaiseChanged(TimeChangeType.Spend, before, Current, -amount, context);
            
            // 0이 되었는지 확인 후 고갈 이벤트 발생
            CheckDepleted(context);

            return true;
        }

        public void Gain(float amount, TimeGainContext context)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "회복량은 음수가 될 수 없습니다.");

            if (amount == 0f)
                return;

            float before = Current;
            Current = Mathf.Clamp(Current + amount, 0f, Max);
            
            // 시간 변화 이벤트 발생
            RaiseChanged(TimeChangeType.Gain, before, Current, Current - before, context);
            
            // 0보다 커지면 다음에 다시 0이 될 때 고갈 이벤트가 재발생 하도록 플래그 해제
            if (Current > 0f) _isDepletedRaised = false;
        }

        public void SetCurrent(float newCurrent, TimeSetContext context)
        {
            float before = Current;
            Current = Mathf.Clamp(newCurrent, 0f, Max);
            
            // 시간 변화 이벤트 발생
            RaiseChanged(TimeChangeType.SetCurrent, before, Current, Current - before, context);
            
            // 0이 되었는지 확인 후 고갈 이벤트 발생
            CheckDepleted(context);
        }

        public void SetMax(float newMax, bool keepRatio, TimeMaxChangedContext context)
        {
            if (newMax <= 0)
                throw new ArgumentOutOfRangeException(nameof(newMax), "최대 시간(Max)은 0보다 커야 합니다.");

            float beforeCurrent = Current;
            float beforeMax = Max;

            if (keepRatio)
            {
                // Max가 바뀌어도 현재 비율(Current / Max)은 유지하도록 보정
                float ratio = (beforeMax <= 0f) ? 1f : (beforeCurrent / beforeMax);
                Max = newMax;
                Current = Mathf.Clamp(Max * ratio, 0f, Max);
            }
            else
            {
                // Max만 바꾸고 Current는 단순히 범위 내로만 보정
                Max = newMax;
                Current = Mathf.Clamp(Current, 0f, Max);
            }

            // Max 변경도 시간 변화 이벤트로 통지
            RaiseChanged(TimeChangeType.SetMax, beforeCurrent, Current, Current - beforeCurrent, context);
            
            // Max만 바뀌어도 현재 남은 시간이 0이면 고갈 상태일 수 있으니 체크 
            CheckDepleted(context);
        }

        private void RaiseChanged(TimeChangeType type, float before, float after, float delta, object context)
        {
            Changed?.Invoke(new TimeChangedArgs(type, before, after, delta, context));
        }

        private void CheckDepleted(object context)
        {
            if (Current > 0f) return;
            if (_isDepletedRaised) return;

            _isDepletedRaised = true;
            Depleted?.Invoke(new TimeDepletedArgs(context));
        }
    }
}
