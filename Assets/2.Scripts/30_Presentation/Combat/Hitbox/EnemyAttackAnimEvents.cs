using System;
using System.Collections;
using UnityEngine;
using Hourbound.Presentation.Feedback.Telegraph;

namespace Hourbound.Presentation.Combat.Hitbox
{
    public sealed class EnemyAttackAnimEvents : MonoBehaviour
    {
        [Header("Hitbox")]
        [SerializeField] private MeleeHitBox hitbox;
        
        [Header("Telegraph")]
        [SerializeField] private EnemyAttackTelegraph telegraph;

        [Header("돌진 거리")]
        [SerializeField] private Rigidbody rb;
        [Min(0f)] [SerializeField] private float lungeSpeed = 6f;
        
        [Header("공격 후 복귀여부")]
        [Tooltip("공격 종료 시 제자리로 복귀에 대한 것.")]
        [SerializeField] private bool returnToStartOnEnd = true;
        
        [Tooltip("복귀 보간 시간. 0이면 즉시 복귀")]
        [Min(0f)] [SerializeField] private float returnDuration = 0.08f;
        
        [Tooltip("복귀 중에는 수평 이동을 강제로 0으로 유지")]
        [SerializeField] private bool zeroHorizontalVelocityWhileReturning = true;
        
        [Header("Failsafe")]
        [Tooltip("End 이벤트가 안 와도 이 시간 후 강제로 End처리(텔레그래프/돌진/히트박스 정리)")]
        [Min(0.05f)] [SerializeField] private float maxLungeWindow = 0.35f;
        
        [SerializeField] private float telegraphDebounce = 0.05f;
        
        [Header("Debug")]
        [SerializeField] private bool log;
        
        private bool _telegraphOn;
        private float _lastTelegraphBeginAt;
        private bool _lunging;
        private Vector3 _startPos;
        private Coroutine _returnCo;
        private Coroutine _failsafeCo;
        
        private void Reset()
        {
            if (hitbox == null) hitbox = GetComponentInChildren<MeleeHitBox>(true);
            if (rb == null) rb = GetComponentInParent<Rigidbody>();
            if (telegraph == null) telegraph = GetComponentInChildren<EnemyAttackTelegraph>(true);
        }

        private void OnDisable()
        {
            // 텔레그래프가 켜져있던 플래그/상태 정리
            _telegraphOn = false;
            if (telegraph != null) telegraph.Hide();
            
            // 히트박스가 켜져있으면 무조건 끄기
            if (hitbox != null) hitbox.Deactivate();
            
            // 돌진 상태 종료
            _lunging = false;
            
            // 코루틴 정리
            if (_returnCo != null)
            {
                StopCoroutine(_returnCo);
                _returnCo = null;
            }

            if (_failsafeCo != null)
            {
                StopCoroutine(_failsafeCo);
                _failsafeCo = null;
            }
            
            // 수평 속도 정리
            if (rb != null)
            {
                Vector3 v = rb.linearVelocity;
                v.x = 0f; v.z = 0f;
                rb.linearVelocity = v;
            }
        }

        private void FixedUpdate()
        {
            if (!_lunging) return;
            if (rb == null) return;

            Vector3 f = transform.root.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 0.0001f) return;
            f.Normalize();

            Vector3 delta = f * (lungeSpeed * UnityEngine.Time.fixedDeltaTime);
            rb.MovePosition(rb.position + delta);
        }

        

        // Animation Event 함수는 Public 권장
        public void AE_AttackHitboxBegin()
        {
            if (_lunging) return;
            
            if (hitbox != null) hitbox.Activate();
            
            if (rb != null)
                _startPos = rb.position;
            
            // 복귀 코루틴이 돌고 있었으면 끊고 다시 시작 위치 갱신
            if (_returnCo != null)
            {
                StopCoroutine(_returnCo);
                _returnCo = null;
            }
            
            _lunging = true;
            
            if (log)
                Debug.Log("[AnimEvent] HitboxBegin + Lunge", this);
            
            if (_failsafeCo != null) StopCoroutine(_failsafeCo);
            _failsafeCo = StartCoroutine(CoFailsafeEnd());
        }

        public void AE_AttackHitboxEnd()
        {
            if (!_lunging) return;

            if (_failsafeCo != null)
            {
                StopCoroutine(_failsafeCo);
                _failsafeCo = null;
            }
            
            // 텔레그래프가 따로 End 이벤트를 못 받는 상황 대비 보험
            if (telegraph != null) telegraph.Hide();
            
            if (hitbox != null) hitbox.Deactivate();
            
            _lunging = false;
            
            if (rb == null) return;

            Vector3 v = rb.linearVelocity;
            v.x = 0f; v.z  = 0f;
            rb.linearVelocity = v;

            if (returnToStartOnEnd)
            {
                if (_returnCo != null) StopCoroutine(_returnCo);
                _returnCo = StartCoroutine(CoReturnToStart());
            }
            
            if (log)
                Debug.Log("[AnimEvents] HitboxEnd + Return", this);
        }

        public void AE_TelegraphBegin()
        {
            float now = UnityEngine.Time.time;
            
            // 너무 빨리 재진입 하면 무시
            if (now - _lastTelegraphBeginAt < telegraphDebounce)
                return;
            
            _lastTelegraphBeginAt = now;
            
            // 이미 켜져 있으면 무시
            if (_telegraphOn) return;
            _telegraphOn = true;
            
            telegraph?.Show();
        }

        public void AE_TelegraphEnd()
        {
            // 이미 꺼져 있으면 무시
            if (!_telegraphOn) return;
            _telegraphOn = false;
            
            telegraph?.Hide();
        }

        private IEnumerator CoReturnToStart()
        {
            //즉시 스냅
            if (returnDuration <= 0f)
            {
                rb.MovePosition(_startPos);
                _returnCo = null;
                yield break;
            }
            
            float t = 0f;
            Vector3 from = rb.position;
            Vector3 to = _startPos;

            while (t < returnDuration)
            {
                t += UnityEngine.Time.fixedDeltaTime;

                if (zeroHorizontalVelocityWhileReturning)
                {
                    Vector3 v = rb.linearVelocity;
                    v.x = 0f; v.z = 0f;
                    rb.linearVelocity = v;
                }
                
                float a = Mathf.Clamp01(t / returnDuration);
                Vector3 p = Vector3.Lerp(from, to, a);
                rb.MovePosition(p);
                
                yield return new WaitForFixedUpdate();
            }
            
            rb.MovePosition(to);
            _returnCo = null;
        }

        private IEnumerator CoFailsafeEnd()
        {
            yield return new WaitForSeconds(maxLungeWindow);
            
            if (!_lunging) { _failsafeCo = null; yield break; }
            
            if (log) Debug.Log("[FailSafe] Force End", this);
            
            // End 이벤트가 안와도 동일하게 정리
            ForceEnd();
            _failsafeCo = null;
        }

        private void ForceEnd()
        {
            if (hitbox != null) hitbox.Deactivate();
            
            _lunging = false;

            if (rb != null)
            {
                var v = rb.linearVelocity;
                v.x = 0f; v.z = 0f;
                rb.linearVelocity = v;

                if (returnToStartOnEnd)
                {
                    if (_returnCo != null) StopCoroutine(_returnCo);
                    _returnCo = StartCoroutine(CoReturnToStart());
                }
            }
            
            telegraph?.Hide();
        }
    }
}