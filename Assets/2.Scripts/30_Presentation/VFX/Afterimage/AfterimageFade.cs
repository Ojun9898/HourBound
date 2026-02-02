using System;
using UnityEngine;

namespace Hourbound.Presentation.Feedback.Afterimage
{
    // 잔상 스프라이트가 일정 시간 동안 페이드아웃 후 사라지게 한다.
    public sealed class AfterimageFade : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Min(0.01f)] [SerializeField] private float lifeSeconds = 0.18f;
        
        private float _startRealtime;
        private Color _startColor;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogError("AfterimageFade : SpriteRenderer가 없습니다.", this);
                    enabled = false;
                    return;
                }
            }
            
            _startColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            _startRealtime = UnityEngine.Time.realtimeSinceStartup;
        }

        private void Update()
        {
            float t = (UnityEngine.Time.realtimeSinceStartup - _startRealtime) / lifeSeconds;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }
            
            float a = Mathf.Lerp(_startColor.a, 0f, t);
            var c = _startColor;
            c.a = a;
            spriteRenderer.color = c;
        }
        
        // 외부에서 초기 세팅을 바꾸고 싶을 때 사용
        public void Set(Sprite sprite, Vector3 position, Quaternion rotation, Vector3 scale, Color color, float life)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = sprite;
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
            
            lifeSeconds = Mathf.Max(0.01f, life);
            _startColor = color;
            
            if (spriteRenderer != null) spriteRenderer.color = _startColor;
        }
    }
}