using System;
using UnityEngine;

namespace Hourbound.Presentation.Feedback
{
    // 여러 SpriteRenderer(부위)를 복제해서 잔상을 그리는 Rig
    public sealed class AfterimageRig : MonoBehaviour
    {
        [Min(0.01f)] [SerializeField] private float lifeSeconds = 0.18f;
        
        private SpriteRenderer[] _parts;
        private float _startRealtime;
        private float _endRealtime;
        private Color[] _startColors;
        
        // 외부에서 현재 프레임의 부위 렌더러 상태를 복제헤 초기화
        public void InitFrom(
            SpriteRenderer[] sourceParts,
            Vector3 position,
            Quaternion rotation,
            Vector3 lossyScales,
            float alpha,
            float life)
        {
            if (sourceParts == null || sourceParts.Length == 0)
            {
                Destroy(gameObject);
                return;
            }
            
            lifeSeconds = Mathf.Max(0.01f, life);
            
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = lossyScales;
            
            EnsureParts(sourceParts.Length);
            
            _startColors = new Color[_parts.Length];

            for (int i = 0; i < _parts.Length; i++)
            {
                var src = sourceParts[i];
                var dst = _parts[i];
                
                // 소스가 비활성 / 없음 / 스프라이트 없음이면 해당 파트는 숨김
                if (src == null || !src.enabled || src.sprite == null)
                {
                    dst.enabled = false;
                    continue;
                }
                
                dst.enabled = true;
                
                // 렌더링 정보 복제
                dst.sprite = src.sprite;
                dst.flipX = src.flipX;
                dst.flipY = src.flipY;
                
                // 정렬 정보 복제
                dst.sortingLayerID = src.sortingLayerID;
                dst.sortingOrder = src.sortingOrder;
                
                // 머티리얼(선택) : 잔상 전용 머티리얼을 쓰고 싶으면 프리팹에서 바꾸면 됨
                dst.sharedMaterial = src.sharedMaterial;
                
                // 색 / 투명도 복제 (알파만 덮어쓰기)
                Color c = src.color;
                c.a *= Mathf.Clamp01(alpha);
                dst.color = c;
                
                _startColors[i] = c;
                
                // 부위별 로컬 트랜스폼 복제(뼈대처럼 움직이는 형태를 유지)
                dst.transform.localPosition = src.transform.localPosition;
                dst.transform.localRotation = src.transform.localRotation;
                dst.transform.localScale = src.transform.localScale;
            }
            
            _startRealtime = UnityEngine.Time.realtimeSinceStartup;
            _endRealtime = _startRealtime + lifeSeconds;
        }

        private void Update()
        {
            if (_parts == null || _parts.Length == 0) return;
            
            float now = UnityEngine.Time.realtimeSinceStartup;
            float t = Mathf.InverseLerp(_startRealtime, _endRealtime, now);

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }
            
            // 모든 파트를 동일한 비율로 페이드아웃
            for (int i = 0; i < _parts.Length; i++)
            {
                var sr = _parts[i];
                if (sr == null || !sr.enabled) continue;
                
                Color c = _startColors[i];
                c.a = Mathf.Lerp(_startColors[i].a, 0f, t);
                sr.color = c;
            }
        }
        
        // 필요한 만큼 자식 spriteRenderer를 만들어 둔다.
        private void EnsureParts(int count)
        {
            if (_parts != null && _parts.Length == count) return;
            
            // 기존 자식 정리
            for (int i = transform.childCount - 1; i >= 0; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            
            _parts = new SpriteRenderer[count];

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"AfterimagePart_{i}");
                go.transform.SetParent(transform, false);
                
                var sr = go.AddComponent<SpriteRenderer>();
                _parts[i] = sr;
            }
        }
    }
}