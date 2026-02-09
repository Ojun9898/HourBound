using UnityEngine;

namespace Hourbound.Application.Rewards
{
    public sealed class EnemyRewardInfo : MonoBehaviour, IKillRewardMultiplier
    {
        [SerializeField] private string sourceId = "Enemy_Slime";
        [SerializeField] private float killRewardMultiplier = 1f; // 엘리트 = 2, 보스 = 5 등
        
        public float KillRewardMultiplier => killRewardMultiplier;
        public string KillSourceId => sourceId;
    }
}