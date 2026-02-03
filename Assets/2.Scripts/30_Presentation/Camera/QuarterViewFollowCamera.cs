using System;
using UnityEngine;

namespace HourBound.Presentation.Camera
{
    /// <summary>
    /// 쿼터뷰(Top-Down/Quarter) 팔로우 카메라
    /// - 타겟을 일정 오프셋에서 바라보며 부드럽게 따라간다.
    /// - 회전은 기본 고정(수동 조절 가능)
    /// </summary>
    public sealed class QuarterViewFollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        
        [Header("View (Quarter)")]
        [Tooltip("카메라가 target 뒤로 얼마나 떨어질지(수평 거리)")]
        [Min(0f)] [SerializeField] private float distance = 8f;
        
        [Tooltip("카메라 높이")]
        [Min(0f)] [SerializeField] private float height = 10f;
        
        [Tooltip("Yaw(좌우 회전). 기본값(쿼터 뷰) = 45")]
        [SerializeField] private float yawDegrees = 45f;
        
        [Tooltip("Pitch(아래를 내려다 보는 각). 50~65 권장")]
        [Range(10f, 89f)] [SerializeField] private float pitchDegrees = 60f;
        
        [Header("Follow")]
        [Tooltip("타겟을 바라볼 때 추가로 더할 오프셋(예 : 캐릭터 가슴 높이)")]
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.2f, 0f);
        
        [Tooltip("카메라 위치 보간 속도 (클수록 빨라짐)")]
        [Min(0f)] [SerializeField] private float positionLerp = 12f;
        
        [Tooltip("카메라 회전 보간 속도 (클수록 빨라짐)")]
        [Min(0f)] [SerializeField] private float rotationLerp = 12f;
        
        [Header("Options")]
        [Tooltip("LateUpdate에서 따라가면 캐릭터 움직임과 더 자연스러움")]
        [SerializeField] private bool useLateUpdate = true;

        private void Reset()
        {
            // 자동 타겟 시도(태그 사용 시)
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        private void Awake()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
        }

        private void Update()
        {
            if (!useLateUpdate) Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (useLateUpdate) Tick(Time.deltaTime);
        }

        private void Tick(float dt)
        {
            if (target == null) return;
            
            // 쿼터뷰 기준 회전(고정)
            Quaternion viewRot = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
            
            // 타겟 기준으로 "뒤쪽" 방향으로 distance만큼, 위로 height만큼
            // (viewRot * Vector3.forward)가 "카메라가 바라보는 정면"이므로, 그 반대 방향이 뒤쪽
            Vector3 back = -(viewRot * Vector3.forward);
            Vector3 desiredPos = target.position + back * distance + Vector3.up * height;
            
            // 부드럽게 위치 이동
            if (positionLerp <= 0) transform.position = desiredPos;
            else transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-positionLerp * dt));
            
            // 타겟을 바라보도록 회전
            Vector3 lookPos = target.position + lookAtOffset;
            Quaternion desiredRot = Quaternion.LookRotation((lookPos - transform.position).normalized, Vector3.up);
            
            if (rotationLerp <= 0f) transform.rotation = desiredRot;
            else transform.rotation= Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-rotationLerp * dt));
        }
        
        /// <summary> 런타임에서 타겟 교체 가능 </summary>
        public void SetTarget(Transform newTarget) => target = newTarget;

        /// <summary> 카메라 기준 이동/닷지 방향 계산에 쓸 수 있는 평면 전방 </summary>
        public Vector3 PlanarForward
        {
            get
            {
                Vector3 f = transform.forward;
                f.y = 0f;
                return f.sqrMagnitude < 0.0001f ? Vector3.forward : f.normalized;
            }
        }

        /// <summary> 카메라 기준 이동/닷지 방향 계산에 쓸 수 있는 평면 우측 </summary>
        public Vector3 PlanarRight
        {
            get
            {
                Vector3 r= transform.right;
                r.y = 0f;
                return r.sqrMagnitude < 0.0001f ? Vector3.right : r.normalized;
            }
        }
    }
}