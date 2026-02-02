using System;
using System.Collections;
using UnityEngine;

namespace Hourbound.Presentation.Player.Motion
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControllerDodgeMotor : MonoBehaviour, IDodgeMotor
    {
        [Header("방향 기준")]
        [Tooltip("카메라 기준으로 입력 방향을 월드 방향으로 변환할지")] 
        [SerializeField] private bool cameraRelative = true;

        [Tooltip("cameraRelative가 켜져있을 때 기준이 되는 Transform (비우면 메인 카메라 사용)")] 
        [SerializeField] private Transform cameraTransform;

        [Tooltip("수평면 기준 측 (보통 Vector3.up)")] 
        [SerializeField] private Vector3 upAxis = default;

        [Header("Rotate")]
        [Tooltip("대시 중 이동방향으로 캐릭터를 회전시킬지")] 
        [SerializeField] private bool rotateTowardMoveDir = true;
        
        [Tooltip("true면 대시 시작 순간에만 회전하고, 대시 중에는 회전을 고정한다.")]
        [SerializeField] private bool rotateOnlyAtStart = true;

        [Tooltip("회전 속도. 값이 클수록 빠르게 돈다.")] [Min(0f)] 
        [SerializeField] private float rotationSpeedDegPerSec = 900f;

        [Tooltip("회전이 너무 딱딱하면 스무딩(0이면 즉시, 1에 가까울수록 느리게")] 
        [Range(0f, 1f)] [SerializeField] private float rotationSmoothing = 0.1f;

        [Header("Collision Slide")]
        [Tooltip("벽/오브젝트에 부딫혔을 때 미끄러지듯 방향을 수정할지")]
        [SerializeField] private bool slideOnHit = true;
        
        [Tooltip("정면으로 박아서 슬라이드 방향이 거의 없으면 대시를 끝낼지")]
        [SerializeField] private bool stopDodgeIfBlocked = true;
        
        [Tooltip("슬라이드 방향이 이 값보다 작으면 막힌 것으로 처리")]
        [Min(0f)] [SerializeField] private float minSlideDirSqr = 0.0004f; // 0.02^2
        
        public bool IsDodging { get; private set; }

        private CharacterController _cc;
        private Coroutine _co;
        
        // CharacterController는 Move결과에 노멀을 주지 않아 OnControllerColliderHit로 노멀을 받아야함
        private Vector3 _lastHitNormal;
        private bool _hitThisFrame;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (upAxis == default) upAxis = Vector3.up;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // 이번 프레임 Move에서 발생한 충돌 노멀 기록
            _lastHitNormal = hit.normal;
            _hitThisFrame = true;
        }

        public void Dodge(Vector3 worldDirNormalized, float distance, float duration)
        {
            if (distance <= 0f) return;
            if (duration <= 0.0001f) duration = 0.0001f;

            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(CoDodge(worldDirNormalized, distance, duration));
        }

        private IEnumerator CoDodge(Vector3 inputDirNormalized, float distance, float duration)
        {
            IsDodging = true;

            // 1 ) 방향 확정
            Vector3 dir = inputDirNormalized;
            
            if (cameraRelative)
                dir = ToCameraRelativeOnPlane(dir);

            // 수평면에 투영
            dir = Vector3.ProjectOnPlane(dir, upAxis).normalized;
            
            // 입력이 없으면 캐릭터 전방 기준
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector3.ProjectOnPlane(transform.forward, upAxis).normalized;

            // 2) 대시 시작 시점에 한 번 회전
            if (rotateTowardMoveDir)
                RotateTowards(dir, snap: true);

            // 3 ) 분할 이동
            float elapsed = 0f;
            float speed = distance / duration;
            
            // 누적 이동량을 제한해서 duration동안 distance를 대체로 맞추기
            float remaining = distance;

            while (elapsed < duration)
            {
                float dt = UnityEngine.Time.unscaledDeltaTime;
                elapsed += dt;

                // 이번 프레임 이동 목표량(남은 거리 고려)
                float stepDist = Mathf.Min(speed * dt, remaining);
                Vector3 step = dir * stepDist;
                
                // 충돌 플래그 초기화
                _hitThisFrame = false;
                
                // 이동
                _cc.Move(step);
                
                remaining -= stepDist;

                // 대시 중에도 계속 회전 (유지할지 말지는 추후 결정)
                if (rotateTowardMoveDir)
                    RotateTowards(dir, snap: false);
                
                // 슬라이드 처리
                if (slideOnHit && _hitThisFrame)
                {
                    // 벽에 부딪혔을 때, 이동 방향을 충돌 노멀의 평면으로 투영해서 "벽을 따라 미끄러지게"
                    Vector3 slideDir = Vector3.ProjectOnPlane(dir, _lastHitNormal);
                    slideDir = Vector3.ProjectOnPlane(slideDir, upAxis).normalized;

                    if (slideDir.sqrMagnitude >= minSlideDirSqr)
                    {
                        dir = slideDir;
                        
                        // 슬라이드 방향으로 회전(대시 중 회전 허용 시)
                        if (rotateTowardMoveDir && !rotateOnlyAtStart)
                            RotateTowards(dir, snap: false);
                    }
                    else
                    {
                        // 거의 정면으로 박은 경우 : 슬라이드가 안되면 대시 종료
                        if (stopDodgeIfBlocked)
                            break;
                    }
                }

                yield return null;
            }

            IsDodging = false;
            _co = null;
        }

        private void RotateTowards(Vector3 moveDirOnPlane, bool snap)
        {
            moveDirOnPlane = Vector3.ProjectOnPlane(moveDirOnPlane, upAxis);
            if (moveDirOnPlane.sqrMagnitude < 0.0001f) return;
            
            Quaternion target = Quaternion.LookRotation(moveDirOnPlane.normalized, upAxis);

            if (snap || rotationSpeedDegPerSec <= 0f)
            {
                transform.rotation = target;
                return;
            }
            
            float dt = UnityEngine.Time.unscaledDeltaTime;
            
            // 1 ) 회전속도 제한
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                rotationSpeedDegPerSec * dt
                );
            
            // 2 ) 추가 스무딩(선택)
            if (rotationSmoothing > 0f)
            {  
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    target,
                    1f - Mathf.Pow(1f - rotationSmoothing, 60 * dt)
                    );
            }
        }

    private Vector3 ToCameraRelativeOnPlane(Vector3 inputDir)
        {
            Transform cam = cameraTransform != null 
                ? cameraTransform 
                : (Camera.main != null ? Camera.main.transform : null);
            
            // 입력 (x, y)를 카메라의 right/forward 로 반환
            // inputDir.x = 좌우, inputDir.z = 전후 (직접 넣을 땐, x, z로 넣는 것을 추천)
            Vector3 right = Vector3.ProjectOnPlane(cam.right, upAxis).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, upAxis).normalized;
            
            Vector3 world = right * inputDir.x + forward * inputDir.z;
            return world.sqrMagnitude < 0.0001f ? inputDir : world.normalized;
        }
    }
}