using UnityEngine;

namespace Hourbound.Presentation.Skills
{
    // 스킬의 "데이터"만 담는 스크립터블 오브젝트
    // (실행 로직은 SkillRunner가 담당)
    [CreateAssetMenu(menuName = "HOURBOUND/스킬/스킬 데이터", fileName = "SO_Skill_")]
    public class SkillDataSO : ScriptableObject
    {
        [Header("식별자")] 
        [SerializeField] private string skillId = "Skill_Test";

        [Header("시간 비용(기본)")]
        [Min(0f)] [SerializeField] private float baseTimeCost = 2f;

        [Header("연속 사용 비용 증가")]
        [Tooltip("이 시간(초) 이상 스킬을 안 쓰면 연속 카운트를 초기화한다.")] 
        [Min(0f)] [SerializeField] private float chainResetSeconds = 1.2f;
        
        [Tooltip("연속 사용 카운트 1 증가당 비용이 얼마나 증가하는지(가산 계수)")]
        [Min(0f)] [SerializeField] private float chainAddMultiplier;
        
        [Tooltip("연속 증가의 곡선(1이면 선형, 2면 더 가파름)")]
        [Range(1f, 3f)] [SerializeField] private float chainCurve = 1.2f;
        
        public string SkillId => skillId;
        public float BaseTimeCost => baseTimeCost;
        public float ChainResetSeconds => chainResetSeconds;
        public float ChainAddMultiplier => chainAddMultiplier;
        public float ChainCurve => chainCurve;
    }
}
