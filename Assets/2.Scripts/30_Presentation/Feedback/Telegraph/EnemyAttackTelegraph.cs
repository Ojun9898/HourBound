using System;
using UnityEngine;

namespace Hourbound.Presentation.Feedback.Telegraph
{
    /// <summary>
    /// 공격 예고(Telegraph)를 바닥에 표시하는 최소 구현
    /// - Quad/Plane 같은 MeshRenderer를 켜고/끄는 방식
    /// TODO : 길이/폭/방향(전방 부채꼴/직선)으로 확장
    /// </summary>
    public sealed class EnemyAttackTelegraph : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform indicatorRoot; // 보통 Quad/Plane의 부모
        [SerializeField] private MeshRenderer indicatorRenderer;
        
        [Header("Placement")]
        [Tooltip("바닥에 살짝 띄워 z-fighting 방지")]
        [Min(0f)] [SerializeField] private float yOffset = 0.02f;
        
        [Tooltip("루트 기준 전방으로 얼마나 앞에 둘지(근접 공격이면 0.8 ~ 1.2 추천")]
        [SerializeField] private float forwardOffset = 1.0f;
        
        [Tooltip("표시 크기(폭, 길이). Quad 기준 localScale(x = 폭, z = 길이)")]
        [SerializeField] private Vector2 size = new Vector2(1.2f, 2.0f);
        
        [Header("Debug")]
        [SerializeField] private bool log;
        
        private Vector3 _baseLocalScale;
        private bool _baseScaleCaptured;

        private void Reset()
        {
            if (indicatorRoot == null) indicatorRoot = transform;
            if (indicatorRenderer == null) indicatorRenderer = GetComponentInChildren<MeshRenderer>();
        }

        private void Awake()
        {
            if (indicatorRoot == null) indicatorRoot = transform;

            if (!_baseScaleCaptured)
            {
                _baseLocalScale = indicatorRoot.localScale;
                _baseScaleCaptured = true;
            }
            
            // 시작은 꺼진 상태 권장
            SetVisible(false);
            ApplySize();
        }

        public void Show()
        {
            if (_baseScaleCaptured && indicatorRoot != null)
                indicatorRoot.localScale = _baseLocalScale;
            
            Place();
            ApplySize();
            SetVisible(true);
            
            if (log) Debug.Log("[Telegraph] show", this);
        }

        public void ShowFor(float seconds)
        {
            Show();
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), seconds);
        }

        public void Hide()
        {
            SetVisible(false);
            
            // 다음 Show 때 누적 안 되도록 스케일 원상 복구
            if (_baseScaleCaptured && indicatorRoot != null)
                indicatorRoot.localScale = _baseLocalScale;
            
            if (log) Debug.Log("[Telegraph] Hide", this);
        }

        private void SetVisible(bool visible)
        {
            if (indicatorRenderer != null)
                indicatorRenderer.enabled = visible;
        }

        private void ApplySize()
        {
            if (indicatorRoot == null) return;
            
            indicatorRoot.localScale = new Vector3(
                _baseLocalScale.x * size.x,
                _baseLocalScale.y,
                _baseLocalScale.z * size.y
                );
        }

        private void Place()
        {
            // Enemy 루트 기준 전방으로 배치
            Transform root = transform.root;
            
            Vector3 f = root.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 0.0001f) f = Vector3.forward;
            f.Normalize();
            
            Vector3 pos = root.position + f * forwardOffset;
            pos.y = root.position.y + yOffset;
            
            // 텔레그래프 자체는 바닥에만 놓고, 회전은 루트 전방을 따라감
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(f, Vector3.up);
        }
    }
}