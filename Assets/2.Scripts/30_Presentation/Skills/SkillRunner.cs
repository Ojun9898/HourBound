using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Time;

namespace Hourbound.Presentation.Skills
{
    // 스킬 사용을 실제로 수행하는 컴포넌트
    // - 입력(Q) 수신
    // - 연속 사용 카운트 관리
    // - 시간 비용 계산 후 TrySpend 호출
    // - 성공 시 스킬 실행(현재는 로그만 출력)
    // - TODO : 이후 이펙트 / 히트박스 연결 
    public sealed class SkillRunner : MonoBehaviour
    {
        [Header("의존성 주입")]
        [SerializeField] private TimeResourcePresenter timePresenter;
        
        [Header("스킬 데이터")]
        [SerializeField] private SkillDataSO skillData;
        
        [Header("입력(디버그 용도)")]
        [SerializeField] private Key castKey = Key.Q;
        
        private ITimeResource _time;

#if UNITY_EDITOR
        private InputAction _castAction;
#endif
        
        private int _chainCount;
        private float _lastCastRealtime;

        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("SkillRunner : TimeResourcePresentation이 연결되지 않았습니다.");
                enabled = false;
                return;
            }

            if (skillData == null)
            {
                Debug.LogError("SkillRunner : SkillDataSO가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            // 키 바인딩을 동적으로 구성( Default : Q )
            _castAction = new InputAction("디버그_스킬사용");
            _castAction.AddBinding($"<Keyboard>/{castKey.ToString().ToLower()}");
#endif
        }

        private void Start()
        {
            _time = timePresenter.Time;

            if (_time == null)
            {
                Debug.LogError("SkillRunner : Time이 아직 준비되지 않았습니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (_castAction != null)
            {
                _castAction.performed += OnCast;
                _castAction.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (_castAction != null)
            {
                _castAction.performed -= OnCast;
                _castAction.Disable();
            }
#endif
        }

#if UNITY_EDITOR
        private void OnCast(InputAction.CallbackContext ctx)
        {
            if (_time == null) return;
            
            // 연속 사용 카운트 갱신 (실시간 기준)
            UpdateChainCount();
            
            float cost = CalculateTimeCost(_chainCount);
            
            // 시간 소모 시도
            bool ok = _time.TrySpend(cost, new TimeSpendContext("스킬사용", skillData.SkillId, _chainCount));
            if (!ok)
            {
                Debug.Log($"스킬 사용 실패 : 시간이 부족합니다. 필요 = {cost:F2}, 현재 남은 시간 = {_time.Current:F2}", this);
                return;
            }
            
            // 성공 시 스킬 실행(현재는 로그)
            Debug.Log($"스킬 사용 성공! : {skillData.SkillId} / 비용 = {cost:F2} / 연속 횟수 = {_chainCount} / 남은 시간 = {_time.Current:F2}", this);
            
            // 마지막 사용 시점 기록
            _lastCastRealtime = UnityEngine.Time.realtimeSinceStartup;
        }
#endif

        private void UpdateChainCount()
        {
            float now = UnityEngine.Time.realtimeSinceStartup;
            
            // 첫 사용이거나, 리셋 시간 이상 지났으면 연속 카운트 초기화
            if (_lastCastRealtime <= 0f || (now - _lastCastRealtime) >= skillData.ChainResetSeconds)
            {
                _chainCount = 0;
                return;
            }
            
            // 연속 카운트 증가
            _chainCount++;
        }

        private float CalculateTimeCost(int chainCount)
        {
            // 기본 비용 * (1 + (연속 카운트 * 계수 ) ^ 상승 곡선)
            // 예 : base = 2, chain = 0 => 2
            //      chain = 1 => 2 * (1 + 0.35^1.2)
            //      chain = 2 => 2 * (2 + 0.35^1.2)
            float add = 0f;

            if (chainCount > 0)
            {
                float x = chainCount * skillData.ChainAddMultiplier;
                add = Mathf.Pow(x, skillData.ChainCurve);
            }
            
            return skillData.BaseTimeCost * (1f + add);
        }
    }
}