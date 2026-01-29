using System;

namespace Hourbound.Domain.Time
{
    // 시간 변화의 종류
    public enum TimeChangeType
    {
        Spend,
        Gain,
        SetCurrent,
        SetMax
    }

    // 시간 변화 이벤트에 전달되는 정보
    public readonly struct TimeChangedArgs
    {
        public readonly TimeChangeType ChangeType;
        public readonly float Before;
        public readonly float After;
        public readonly float Delta;
        public readonly object Context; // 소모/회복/설정의 추가 정보(원인, 출처 등)

        public TimeChangedArgs(TimeChangeType type, float before, float after, float delta, object context)
        {
            ChangeType = type;
            Before = before;
            After = after;
            Delta = delta;
            Context = context;
        }
    }

    // 시간이 0이 되었을 때(고갈) 발생하는 이벤트 정보
    public readonly struct TimeDepletedArgs
    {
        public readonly object Context;
        public TimeDepletedArgs(object context) => Context = context;
    }

    // 시간 소모의 원인을 남기기 위한 컨텍스트
    public readonly struct TimeSpendContext
    {
        public readonly string Reason;    // 예: "Skill.Cast", "Dodge"
        public readonly string SourceId;  // 예: "Skill_Fireball"
        public readonly int ChainCount;   // 연속 사용 스택 등(없으면 0)

        public TimeSpendContext(string reason, string sourceId = null, int chainCount = 0)
        {
            Reason = reason;
            SourceId = sourceId;
            ChainCount = chainCount;
        }
    }

    // 시간 회복의 원인을 남기기 위한 컨텍스트
    public readonly struct TimeGainContext
    {
        public readonly string Reason;     // 예: "Kill", "Finish", "PerfectDodge"
        public readonly string SourceId;   // 예: "Enemy_Slime"
        public readonly float Multiplier;  // 보정치(없으면 1)

        public TimeGainContext(string reason, string sourceId = null, float multiplier = 1f)
        {
            Reason = reason;
            SourceId = sourceId;
            Multiplier = multiplier;
        }
    }

    // 현재 시간을 강제 설정할 때의 컨텍스트
    public readonly struct TimeSetContext
    {
        public readonly string Reason; // 예: "RunStart", "Debug"
        public TimeSetContext(string reason) => Reason = reason;
    }

    // 최대 시간을 변경할 때의 컨텍스트
    public readonly struct TimeMaxChangedContext
    {
        public readonly string Reason; // 예: "Relic", "LevelUp"
        public TimeMaxChangedContext(string reason) => Reason = reason;
    }
}
