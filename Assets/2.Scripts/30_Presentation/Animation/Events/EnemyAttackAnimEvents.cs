using System;
using UnityEngine;
using Hourbound.Presentation.Combat.Hitbox;

namespace Hourbound.Presentation.Combat
{
    // 애니메이션 이벤트를 받아서 히트박스를 제어하는 브릿지
    public sealed class EnemyAttackAnimEvents : MonoBehaviour
    {
        [SerializeField] private MeleeHitBox meleeHitbox;

        private void Awake()
        {
            if (meleeHitbox == null)
            {
                Debug.LogError("EnemyAttackAnimEvents : MeleeHitbox가 연결되지 않았습니다.", this);
                enabled = false;
            }
        }

        // 애니메이션 이벤트에서 호출(이름 정확히 맞추기)
        private void AE_MeleeHitboxBegin()
        {
            meleeHitbox.Begin();
        }
        
        // 애니메이션 이벤트에서 호출(이름 정확히 맞추기)
        private void AE_MeleeHitboxEnd()
        {
            meleeHitbox.End();
        }
    }
}