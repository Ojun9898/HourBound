using System;
using Hourbound.Presentation.Player;
using UnityEngine;

namespace Hourbound.Presentation.Animation
{
    /// <summary>
    /// PlayerMovementController의 현재 이동 속도를 Animator 파라미터(MoveSpeed)에 반영한다.
    /// - 0~1로 정규화해서 BlendTree(Idle/Walk/Run)를 구동하는 용도
    /// - 코드 이동(CharacterController) 기반이므로 RootMotion은 OFF를 권장
    /// </summary>
    public sealed class PlayerLocomotionAnimBinder : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private Animator animator;
        
        [Header("Turning")]
        [Tooltip("이동 최대 속도(m/s). 현재 속도를 0~1로 정규화 할 때 사용")]
        [SerializeField] private float maxMoveSpeed = 6f;
        
        [Tooltip("미세 속도 노이즈 방지 (이 값 이하면 0으로 스냅)")]
        [SerializeField] private float deadzone01 = 0.05f;
        
        [Tooltip("파라미터 변화 부드럽게(초). 0이면 즉시 반영")]
        [SerializeField] private float dampTime = 0.08f;
        
        [Header("Animator Params")]
        [SerializeField] private string moveSpeedParam = "MoveSpeed";
        
        private int _moveSpeedHash;

        private void Awake()
        {
            _moveSpeedHash = Animator.StringToHash(moveSpeedParam);
            
            if (movementController == null)
                movementController = GetComponentInChildren<PlayerMovementController>(true);
            
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }

        private void Update()
        {
            if (movementController == null || animator == null || maxMoveSpeed <= 0.01f)
                return;
            
            float max = movementController.MaxMoveSpeed;
            if (max <= 0.01f) return;
            
            float speed01 = Mathf.Clamp01(movementController.CurrentPlanarSpeed / max);
            if (speed01 < deadzone01) speed01 = 0f;
            
            animator.SetFloat(_moveSpeedHash, speed01, dampTime, UnityEngine.Time.deltaTime);
        }
    }
}