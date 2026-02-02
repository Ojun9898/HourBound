using System;
using UnityEngine;
using Hourbound.Presentation.Player;
using Unity.Collections.LowLevel.Unsafe;

namespace Hourbound.Presentation.Feedback.Afterimage
{
    // 대시 시작 이벤트를 구독해서 잔상을 여러 장 뿌린다.
    public sealed class DashAfterimageSpawner : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private DodgeController dodge;
        [SerializeField] private SpriteRenderer targetSprite;
        
        [Header("잔상 프리팹")]
        [SerializeField] private AfterimageFade afterimagePrefab;
        
        [Header("잔상 설정")]
        [Min(1)] [SerializeField] private int count = 4;
        [Min(0.01f)] [SerializeField] private float intervalSeconds = 0.03f;
        [Min(0.01f)] [SerializeField] private float lifeSeconds = 0.18f;
        
        [Header("색/투명도")]
        [Range(0f, 1f)] [SerializeField] private float alpha = 0.55f;
        
        private int _remaining;
        private float _nextSpawnRealtime;
        private bool _spawning;

        private void Awake()
        {
            if (dodge == null)
            {
                Debug.LogError("DashAfterimageSpawner : DodgeController가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            if (targetSprite == null)
                targetSprite = dodge.GetComponentInChildren<SpriteRenderer>();

            if (targetSprite == null)
            {
                Debug.LogError("DashAfterimageSpawner : 대상 SpriteRenderer를 찾지 못했습니다.");
                enabled = false;
                return;
            }

            if (afterimagePrefab == null)
            {
                Debug.LogError("DashAfterimageSpawner : Afterimage 프리팹이 연결되지 않았습니다.");
                enabled = false;
                return;
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
            
            // 시작 즉시 1장 즉시 스폰
            SpawnOne();
            _remaining--;
            _nextSpawnRealtime = UnityEngine.Time.realtimeSinceStartup + intervalSeconds;
        }

        private void SpawnOne()
        {
            var sprite = targetSprite.sprite;
            if (sprite == null) return;
            
            var inst = Instantiate(afterimagePrefab);
            var color = targetSprite.color;
            color.a = alpha;
            
            inst.Set(
                sprite,
                targetSprite.transform.position,
                targetSprite.transform.rotation,
                targetSprite.transform.lossyScale,
                color,
                lifeSeconds
                );
            
            // 정렬을 맞추고 싶으면 여기서 sortingLayer/order를 복사
            // (프리팹의 SpriteRenderer를 가져와서 targetSprite 기준으로 복사)
        }
    }
}