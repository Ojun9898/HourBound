using System;
using Hourbound.Presentation.Player.State;
using UnityEngine;

namespace Hourbound.Presentation.Player.Combat
{
    /// <summary>
    /// 플레이어 공격 애니메이션 이벤트 수신자.
    /// - AE_AttackHitboxBegin/End 이벤트로 히트박스를 On/Off 한다.
    /// - AE_AttackFinished : FSM에 공격 종료 통지(Locomotion 복귀)
    /// </summary>
    public sealed class PlayerAttackAnimEvents : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameObject hitboxRoot;
        [SerializeField] private PlayerFsmOrchestrator fsm;

        private void Awake()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
        }

        private void OnDisable()
        {
            // 애니메이션 전이/비활성화로 이벤트가 꼬여도 판정이 남지 않도록 하는 안전장치
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
        }

        /// <summary>
        /// (애니메이션 이벤트) 공격 판정 시작.
        /// 클립 타임 0초는 피하고 0.01 ~ 0.05초에 두는 걸 권장.
        /// </summary>
        public void AE_AttackHitboxBegin()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(true);
            
            // 필요하면 FSM에 '공격 윈도우 시작'을 알릴 수 있음(현재는 선택)
            fsm?.NotifyAttackWindowBegan();
        }

        /// <summary>
        /// (애니메이션 이벤트) 공격 판정 종료.
        /// </summary>
        public void AE_AttackHitboxEnd()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
            
            fsm?.NotifyAttackWindowEnded();
        }

        /// <summary>
        /// (애니메이션 이벤트) 공격 애니메이션 종료 (복귀 타이밍).
        /// AttackState 종료 신호로 사용한다.
        /// </summary>
        public void AE_AttackFinished()
        {
            if (hitboxRoot != null)
                hitboxRoot.SetActive(false);
            
            fsm?.NotifyAttackFinished();
        }
    }
}