using Hourbound.Presentation.Player.Motion;
using UnityEngine;

namespace Hourbound.Presentation.Player
{
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform cameraTransform; // 비우면 Camera.main
        [SerializeField] private MonoBehaviour motorBehavior; // IDodgeMotor (대시 중 이동 잠금용)
        
        [Header("Move")]
        [Min(0f)] [SerializeField] private float moveSpeed = 5.5f;
        [Min(0f)] [SerializeField] private float acceleration = 40f;
        [Min(0f)] [SerializeField] private float deceleration = 50f;
        
        [Header("Rotate")]
        [SerializeField] private bool rotateTowardsMoveDir = true;
        [Min(0f)] [SerializeField] private float rotationSpeedDegPerSec = 900f;
        
        [Header("Plane")]
        [SerializeField] private Vector3 upAxis = default; // 보통 Vector3.up
        
        private CharacterController _cc;
        private IDodgeMotor _motor;
        
        private Vector2 _moveInput;
        private Vector3 _planarVelocity; // xz 평면 속도
        
        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (upAxis == default) upAxis = Vector3.up;
            
            _motor = motorBehavior as IDodgeMotor;;
            // motorBehaviour가 비어 있어도 이동은 되게 두고(경고만 함), 잠금만 비활성
            if (motorBehavior != null && _motor == null)
                Debug.LogWarning("PlayerMovementController : motorBehaviour가 IDodgeMotor가 아닙니다.", this);
        }

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        private void Update()
        {
            // 닷지 중에는 기본 이동을 막는게 보통 더 깔끔함.
            if (_motor != null && _motor.IsDodging)
            {
                _planarVelocity = Vector3.zero;
                return;
            }
            
            Vector3 desiredDir = ToCameraRelativeWorldDir(_moveInput);
            Vector3 desiredVel = desiredDir * moveSpeed;
            
            // 가/감속(부드러운 조작감을 위함)
            float accel = (desiredVel.sqrMagnitude > 0.0001f) ? acceleration : deceleration;
            _planarVelocity = Vector3.MoveTowards(_planarVelocity, desiredVel, accel * UnityEngine.Time.deltaTime);
            
            // 이동
            Vector3 delta = _planarVelocity * UnityEngine.Time.deltaTime;
            _cc.Move(delta);
            
            // 회전
            if (rotateTowardsMoveDir && _planarVelocity.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(_planarVelocity.normalized, upAxis);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    target,
                    rotationSpeedDegPerSec * UnityEngine.Time.deltaTime
                    );
            }
        }

        private Vector3 ToCameraRelativeWorldDir(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f) return Vector3.zero;
            
            Transform cam = cameraTransform != null
                ? cameraTransform
                : (Camera.main != null ? Camera.main.transform : null);
            
            // 카메라가 없으면 월드 기준(x=좌우, z=전후)
            if (cam == null)
                return new Vector3(input.x, 0f, input.y).normalized;
            
            Vector3 right = Vector3.ProjectOnPlane(cam.right, upAxis).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, upAxis).normalized;
            
            Vector3 world = right * input.x + forward * input.y;
            world = Vector3.ProjectOnPlane(world, upAxis).normalized;
            return world;
        }
    }
}