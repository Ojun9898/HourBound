using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hourbound.Presentation.Combat.Hitbox
{
    /// <summary>
    /// 테스트용 : 키 입력으로 히트박스 공격 윈도우를 열어준다.
    /// TODO : 애니메이션 이벤트/AI가 Activate/Deactivate를 호출하게 교체하면 됨
    /// </summary>
    public sealed class HitboxWindowActivator : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MeleeHitBox hitbox;
        
        [Header("Test Input")]
        [SerializeField] private Key activationKey = Key.J;
        
        [Header("Window")]
        [Min(0.01f)] [SerializeField] private float activeDuration = 0.12f;

        private void Reset()
        {
            if (hitbox == null) hitbox = GetComponentInChildren<MeleeHitBox>();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb[activationKey].wasPressedThisFrame)
            {
                if (hitbox != null)
                    hitbox.ActiveFor(activeDuration);
            }
        }
    }
}