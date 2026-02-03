using System;
using UnityEngine;
using Hourbound.Presentation.Player.Motion;

namespace Hourbound.Presentation.Player
{
    public enum InputChannel
    {
        Gameplay, // 이동/공격/점프/스킬/닷지 등
        System // Pause, 메뉴 토글 창(잠금 예외로 둘 채널)
    }
    
    public sealed class PlayerActionGate : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MonoBehaviour dodgeMotorBehaviour; // IDodgeMotor
        private IDodgeMotor _dodgeMotor;
        
        [Header("Lock Rules")]
        [SerializeField] private bool lockGameplayWhileDodging = true;

        private void Awake()
        {
            _dodgeMotor = dodgeMotorBehaviour as IDodgeMotor;
            if (dodgeMotorBehaviour != null && _dodgeMotor == null)
                Debug.LogError("PlayerActionGate : dodgeMotorBehaviour가 IDodgeMotor가 아닙니다.", this);
        }

        /// <summary>
        /// 입력 채널 별로 허용/차단을 판정한다.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool IsBlocked(InputChannel channel)
        {
            // System(예 : Pause)은 잠금 예외
            if (channel == InputChannel.System)
                return false;
            
            // Gameplay 잠금 룰
            if (lockGameplayWhileDodging && _dodgeMotor != null && _dodgeMotor.IsDodging)
                return true;
            
            return false;
        }
    }
}