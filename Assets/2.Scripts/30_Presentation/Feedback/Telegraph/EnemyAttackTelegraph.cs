using UnityEngine;

namespace Hourbound.Presentation.Feedback.Telegraph
{
    /// <summary>
    /// 공격 예고(Telegraph)를 바닥에 표시하는 "상태 머신형" 구현.
    /// - Show/Hide를 코루틴/Invoke로 섞지 않고, "요청(Request)"만 받아서 Update에서 상태로 처리한다.
    /// - 이 방식은 애니메이션 이벤트 중복/전이/루프 등으로 Show가 여러 번 호출돼도 마지막 상태만 남아 깜빡임이 원천 차단된다.
    ///
    /// 전제:
    /// - indicatorRoot는 Plane/커스텀 메시(길이 축=Z)를 사용한다.
    ///   width = localScale.x
    ///   length = localScale.z  ✅
    /// </summary>
    public sealed class EnemyAttackTelegraph : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform indicatorRoot;     // Plane Transform 권장
        [SerializeField] private MeshRenderer indicatorRenderer;

        [Header("Placement")]
        [Tooltip("바닥에 살짝 띄워 z-fighting 방지")]
        [Min(0f)] [SerializeField] private float yOffset = 0.02f;

        [Tooltip("루트 기준 전방으로 얼마나 앞에 둘지")]
        [SerializeField] private float forwardOffset = 1.0f;

        [Header("Base Size (scale)")]
        [Tooltip("폭/길이 배율. 폭=x, 길이=y(→ Z축 적용)")]
        [SerializeField] private Vector2 size = new Vector2(1.2f, 2.0f);

        [Header("Debug")]
        [SerializeField] private bool log;

        // ===== base scale =====
        private Vector3 _baseLocalScale;
        private bool _captured;

        // ===== world-length capture (after ApplySize) =====
        private bool _lengthCaptured;
        private Vector3 _defaultDisplayLocalScale;  // ApplySize 후 localScale
        private float _defaultWorldLengthMeters;    // 위 상태에서 월드 길이(m)
        private MeshFilter _mf;

        // ===== request-driven state =====
        private bool _requestedVisible;
        private float _requestedHideAtUnscaled = -1f;   // <0이면 무기한
        private float _requestedAddLengthMeters = 0f;   // 0이면 기본 길이

        // ===== anti-flicker =====
        private int _lastHideFrame = -999;
        [SerializeField] private float showIgnoreAfterHideSeconds = 0.05f; // 0.03 ~ 0.10사이 추천
        private float _ignoreShowUntilUnscaled = -1f;
        
        private void Reset()
        {
            if (indicatorRenderer == null) indicatorRenderer = GetComponentInChildren<MeshRenderer>(true);
            if (indicatorRoot == null && indicatorRenderer != null) indicatorRoot = indicatorRenderer.transform;
            if (indicatorRoot == null) indicatorRoot = transform;
        }

        private void Awake()
        {
            if (indicatorRenderer == null) indicatorRenderer = GetComponentInChildren<MeshRenderer>(true);
            if (indicatorRoot == null)
                indicatorRoot = indicatorRenderer != null ? indicatorRenderer.transform : transform;

            CaptureBaseIfNeeded();
            ApplySize();
            CaptureLengthBaseIfNeeded();

            // 시작은 꺼진 상태
            SetVisible(false);
        }

        private void OnDisable()
        {
            // 어떤 상황에서도 비활성화되면 확실히 OFF
            _requestedVisible = false;
            _requestedHideAtUnscaled = -1f;
            _requestedAddLengthMeters = 0f;

            SetVisible(false);
            ResetWorldLengthToDefault();
        }

        private void Update()
        {
            // 만료되면 자동 OFF (unscaled 기준)
            if (_requestedVisible && _requestedHideAtUnscaled >= 0f && UnityEngine.Time.unscaledTime >= _requestedHideAtUnscaled)
            {
                _requestedVisible = false;
                _requestedHideAtUnscaled = -1f;
                _requestedAddLengthMeters = 0f;

                SetVisible(false);
                ResetWorldLengthToDefault();

                if (log) Debug.Log($"[Telegraph] AutoHide t={UnityEngine.Time.unscaledTime:F3}", this);
                return;
            }

            if (!_requestedVisible)
                return;

            // Visible이면 "현재 상태"를 계속 유지/갱신
            Place();

            if (_requestedAddLengthMeters > 0f)
                SetAdditiveWorldLengthMeters(_requestedAddLengthMeters);
            else
                ResetWorldLengthToDefault();

            SetVisible(true);
        }

        // ===================== Public Request API =====================

        /// <summary>
        /// 텔레그래프를 seconds 동안 표시하도록 요청(Realtime 기준).
        /// addLengthMeters 만큼 길이를 추가(월드 m).
        /// </summary>
        public void RequestShowForRealtime(float seconds, float addLengthMeters)
        {
            // Hide 직후(같은 프레임 OR 아주 짧은 시간)는 Show 무시
            if (UnityEngine.Time.frameCount == _lastHideFrame) return;
            if (_ignoreShowUntilUnscaled >= 0f && UnityEngine.Time.unscaledTime < _ignoreShowUntilUnscaled) return;
            
            _requestedVisible = true;
            _requestedHideAtUnscaled = UnityEngine.Time.unscaledTime + Mathf.Max(0.01f, seconds);
            _requestedAddLengthMeters = Mathf.Max(0f, addLengthMeters);

            if (log) Debug.Log($"[Telegraph] RequestShow sec={seconds:F2} add={addLengthMeters:F2} t={UnityEngine.Time.unscaledTime:F3}", this);
        }

        /// <summary>
        /// 텔레그래프를 즉시 숨기도록 요청.
        /// </summary>
        public void RequestHide()
        {
            _requestedVisible = false;
            _requestedHideAtUnscaled = -1f;
            _requestedAddLengthMeters = 0f;
            
            _lastHideFrame = UnityEngine.Time.frameCount;
            _ignoreShowUntilUnscaled = UnityEngine.Time.unscaledTime + showIgnoreAfterHideSeconds;

            SetVisible(false);
            ResetWorldLengthToDefault();

            if (log) Debug.Log($"[Telegraph] RequestHide t={UnityEngine.Time.unscaledTime:F3}", this);
        }

        // ===================== Internals =====================

        private void SetVisible(bool visible)
        {
            if (indicatorRenderer != null)
                indicatorRenderer.enabled = visible;
        }

        private void CaptureBaseIfNeeded()
        {
            if (_captured) return;
            if (indicatorRoot == null) return;

            _baseLocalScale = indicatorRoot.localScale;
            _captured = true;
        }

        private void ApplySize()
        {
            if (indicatorRoot == null) return;
            if (!_captured) CaptureBaseIfNeeded();

            // Plane(길이축 Z)
            indicatorRoot.localScale = new Vector3(
                _baseLocalScale.x * size.x, // 폭
                _baseLocalScale.y,          // 두께/높이 유지
                _baseLocalScale.z * size.y  // 길이 ✅
            );
        }

        private void CaptureLengthBaseIfNeeded()
        {
            if (_lengthCaptured) return;
            if (indicatorRoot == null || indicatorRenderer == null) return;

            _mf = indicatorRenderer.GetComponent<MeshFilter>();
            _defaultDisplayLocalScale = indicatorRoot.localScale;

            float meshLocalZ = 1f;
            if (_mf != null && _mf.sharedMesh != null)
                meshLocalZ = Mathf.Max(0.0001f, _mf.sharedMesh.bounds.size.z);

            float lossyZ = Mathf.Max(0.0001f, indicatorRoot.lossyScale.z);
            _defaultWorldLengthMeters = Mathf.Max(0.0001f, meshLocalZ * lossyZ);

            _lengthCaptured = true;
        }

        private void SetAdditiveWorldLengthMeters(float extraMeters)
        {
            CaptureLengthBaseIfNeeded();
            float target = Mathf.Max(0.01f, _defaultWorldLengthMeters + Mathf.Max(0f, extraMeters));
            SetWorldLengthMeters(target);
        }

        private void SetWorldLengthMeters(float targetWorldMeters)
        {
            CaptureLengthBaseIfNeeded();

            float target = Mathf.Max(0.01f, targetWorldMeters);
            float ratio = target / _defaultWorldLengthMeters;

            if (indicatorRoot == null) return;

            var s = indicatorRoot.localScale;
            indicatorRoot.localScale = new Vector3(s.x, s.y, _defaultDisplayLocalScale.z * ratio);
        }

        private void ResetWorldLengthToDefault()
        {
            CaptureLengthBaseIfNeeded();
            if (indicatorRoot == null) return;

            var s = indicatorRoot.localScale;
            indicatorRoot.localScale = new Vector3(s.x, s.y, _defaultDisplayLocalScale.z);
        }

        private void Place()
        {
            Transform root = transform.root;

            Vector3 f = root.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 0.0001f) f = Vector3.forward;
            f.Normalize();

            Vector3 pos = root.position + f * forwardOffset;
            pos.y = root.position.y + yOffset;

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(f, Vector3.up);
        }
    }
}
