using System;
using UnityEngine;

namespace Hourbound.Presentation.Combat.Hitbox
{
    // 히트박스 콜라이더의 모양을 런타임에 바꾸는 도구
    public sealed class MeleeHitboxShape : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D box;

        private void Awake()
        {
            if (box == null)
            {
                box = GetComponent<BoxCollider2D>();
                if (box ==null)
                {
                    Debug.LogError("MeleeHitboxShape : BoxCollider2D가 없습니다.", this);
                    enabled = false;
                }
            }
        }
        // 애니메이션 이벤트에서 호출 가능
        // TODO : 프리셋(SO)을 만들어 AE_SetPreset1() 이런식으로 발전시켜야함.
        public void SetBox(float sizeX, float sizeY, float offsetX, float offsetY)
        {
            if (!enabled) return;
            box.size = new Vector2(sizeX, sizeY);
            box.offset = new Vector2(offsetX, offsetY);
        }
    }
}