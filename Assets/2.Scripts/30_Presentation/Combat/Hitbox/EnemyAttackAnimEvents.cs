using System.Collections;
using UnityEngine;
using Hourbound.Presentation.Feedback.Telegraph;

namespace Hourbound.Presentation.Combat.Hitbox
{
    public sealed class EnemyAttackAnimEvents : MonoBehaviour
    {
        [Header("Hitbox")]
        [SerializeField] private MeleeHitBox hitbox;

        [Header("Telegraph (Request-driven)")]
        [SerializeField] private EnemyAttackTelegraph telegraph;
        [Min(0.01f)] [SerializeField] private float telegraphShowFor = 0.22f;

        [Header("Lunge (MovePosition)")]
        [SerializeField] private Rigidbody rb;
        [Min(0f)] [SerializeField] private float lungeSpeed = 6f;
        [Min(0.01f)] [SerializeField] private float maxLungeWindow = 0.25f;

        [Header("Return")]
        [SerializeField] private bool returnToStartOnEnd = true;
        [Min(0f)] [SerializeField] private float returnDuration = 0.08f;
        [SerializeField] private bool zeroHorizontalVelocityWhileReturning = true;

        [Header("Debug")]
        [SerializeField] private bool log;

        private int _lastTelegraphBeginFrame = -999;
        private bool _lunging;
        private float _lungeEndAt;
        private Vector3 _startPos;
        private Coroutine _returnCo;

        private void Reset()
        {
            if (hitbox == null) hitbox = GetComponentInChildren<MeleeHitBox>(true);
            if (telegraph == null) telegraph = GetComponentInChildren<EnemyAttackTelegraph>(true);
            if (rb == null) rb = GetComponentInParent<Rigidbody>();
        }

        private void OnDisable()
        {
            _lunging = false;
            if (_returnCo != null) { StopCoroutine(_returnCo); _returnCo = null; }

            hitbox?.Deactivate();
            telegraph?.RequestHide();
        }

        private void FixedUpdate()
        {
            if (!_lunging) return;
            if (rb == null) return;

            if (UnityEngine.Time.time >= _lungeEndAt)
            {
                EndLunge();
                return;
            }

            Vector3 f = transform.root.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 0.0001f) return;
            f.Normalize();

            Vector3 step = f * (lungeSpeed * UnityEngine.Time.fixedDeltaTime);
            rb.MovePosition(rb.position + step);
        }

        // ---------------- Animation Events ----------------

        public void AE_TelegraphBegin()
        {
            if (UnityEngine.Time.frameCount == _lastTelegraphBeginFrame) return;
            _lastTelegraphBeginFrame = UnityEngine.Time.frameCount;
            
            // "정확한 계획 거리": FixedUpdate 스텝 수 기준
            float dt = UnityEngine.Time.fixedDeltaTime;
            int steps = Mathf.CeilToInt(maxLungeWindow / dt);
            float plannedLungeDistance = steps * lungeSpeed * dt;

            // ✅ ShowForRealtime/Invoke/코루틴을 쓰지 말고 "요청"만 한다.
            telegraph?.RequestShowForRealtime(telegraphShowFor, plannedLungeDistance);

            if (log) Debug.Log($"[AnimEvents] TelegraphBegin steps={steps} plannedDist={plannedLungeDistance:F3}", this);
        }

        public void AE_AttackHitboxBegin()
        {
            telegraph?.RequestHide();
            hitbox?.Activate();

            if (rb != null)
            {
                _startPos = rb.position;
                _lungeEndAt = UnityEngine.Time.time + maxLungeWindow;

                if (_returnCo != null) { StopCoroutine(_returnCo); _returnCo = null; }
                _lunging = true;
            }

            if (log) Debug.Log("[AnimEvents] HitboxBegin + LungeStart", this);
        }

        public void AE_AttackHitboxEnd()
        {
            hitbox?.Deactivate();
            telegraph?.RequestHide();

            EndLunge();

            if (log) Debug.Log("[AnimEvents] HitboxEnd + LungeEnd(+Return)", this);
        }

        // ---------------- Internals ----------------

        private void EndLunge()
        {
            if (!_lunging) return;
            _lunging = false;

            if (rb == null) return;

            if (returnToStartOnEnd)
            {
                if (_returnCo != null) StopCoroutine(_returnCo);
                _returnCo = StartCoroutine(CoReturnToStart());
            }
        }

        private IEnumerator CoReturnToStart()
        {
            if (rb == null)
            {
                _returnCo = null;
                yield break;
            }

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
                    var v = rb.linearVelocity;
                    v.x = 0f; v.z = 0f;
                    rb.linearVelocity = v;
                }

                float a = Mathf.Clamp01(t / returnDuration);
                rb.MovePosition(Vector3.Lerp(from, to, a));

                yield return new WaitForFixedUpdate();
            }

            rb.MovePosition(to);
            _returnCo = null;
        }
    }
}
