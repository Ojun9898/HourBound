namespace Hourbound.Application.Rewards
{ 
    public interface IKillRewardMultiplier
    {
        float KillRewardMultiplier { get; }
        string KillSourceId { get; } // 선택 : EnemyTypeId 같은 걸로 고정 가능
    }
}