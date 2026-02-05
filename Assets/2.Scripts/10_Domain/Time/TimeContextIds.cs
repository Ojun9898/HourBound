namespace Hourbound.Domain.Time
{
    /// <summary>
    /// 시간 변화 Context 문자열(Reason/SourceId) 표준값 모음.
    /// - 오타 방지 + 로그/분석 일관성 유지 목적
    /// </summary>
    public static class TimeReason
    {
        public const string AutoDrain = "자동감소";
        public const string Hit = "피격";
        public const string Skill = "스킬사용";
        public const string Dodge = "회피";
        public const string PerfectDodge = "퍼펙트회피";
        public const string Kill = "처치";
    }
    
    public static class TimeSource
    {
        public const string PerSecond = "초당감소";
        public const string Player = "플레이어";
        public const string EnemyHitBox = "적히트박스";
        public const string EnemyProjectile = "적투사체";
        public const string Trap = "함정";
    }
}