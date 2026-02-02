using System;
using UnityEngine;
using Hourbound.Presentation.Player;

namespace Hourbound.Presentation.Feedback
{
    // 대시 시작 이벤트를 구독해서 부위 전체 잔상을 여러 장 생성한다.
    public sealed class DashAfterimageSpawner_Multi : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private DodgeController dodge;
        [SerializeField] private Transform targetRoot; // 프리팹의 Root 또는 UnitRoot

        [Header("잔상 프리팹")]
        [SerializeField] private AfterimageRig afterimageRigPrefab;

        [Header("잔상 설정")]
        [Min(1)] [SerializeField] private int count = 4;
        [Min(0.01f)] [SerializeField] private float intervalSeconds = 0.03f;
        [Min(0.01f)] [SerializeField] private float lifeSeconds = 0.18f;
        
        [Header("투명도")]
        [Range(0f, 1f)] [SerializeField] private float alpha = 0.55f;
        
        private SpriteRenderer[] _sourceParts;
        
        private int _remaining;
        private float _nextSpawnRealtime;
        private bool _spawning;

        private void Awake()
        {
            if (dodge == null)
            {
                Debug.LogError("DashAfterimageSpawner_Multi : DodgeController가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

            if (targetRoot == null)
            {
                Debug.LogError("DashAfterimageSpawner_Multi : targetRoot가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

            if (afterimageRigPrefab == null)
            {
                Debug.LogError("DashAfterimageSpawner_Multi : AfterimageRig 프리팹이 연결되지 않았습니다.");
                enabled = false;
                return;
            }
            
            // 플레이어 오브젝트 하위의 모든 SpriteRenderer 수집(부위 전체)
            _sourceParts = targetRoot.GetComponentsInChildren<SpriteRenderer>(true);

            if (_sourceParts == null || _sourceParts.Length == 0)
            {
                Debug.LogError("DashAfterimageSpawner_Multi : targetRoot 하위에서 SpriteRenderer를 찾지 못했습니다.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            dodge.DodgeStarted += HandleDashStarted;
        }

        private void OnDisable()
        {
            dodge.DodgeStarted -= HandleDashStarted;
        }

        private void Update()
        {
            if (!_spawning) return;

            if (_remaining <= 0)
            {
                _spawning = false;
                return;
            }
            
            if (UnityEngine.Time.realtimeSinceStartup < _nextSpawnRealtime) return;
            
            SpawnOne();
            _remaining--;
            _nextSpawnRealtime = UnityEngine.Time.realtimeSinceStartup + intervalSeconds;
        }

        private void HandleDashStarted()
        {
            _remaining = Mathf.Max(1, count);
            _nextSpawnRealtime = UnityEngine.Time.realtimeSinceStartup;
            _spawning = true;
            
            // 시작 즉시 1개 소환
            SpawnOne();
            _remaining--;
            _nextSpawnRealtime = UnityEngine.Time.realtimeSinceStartup + intervalSeconds;
        }

        private void SpawnOne()
        {
            if (_sourceParts == null || _sourceParts.Length == 0) return;
            
            var inst = Instantiate(afterimageRigPrefab);
            
            // 캐릭터 전체를 감싸는 루트 기준으로 복제
            inst.InitFrom(
                _sourceParts,
                targetRoot.position,
                targetRoot.rotation,
                targetRoot.lossyScale,
                alpha,
                lifeSeconds);
        }
    }
}