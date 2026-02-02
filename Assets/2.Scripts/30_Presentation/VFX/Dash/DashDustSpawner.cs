using System;
using UnityEngine;
using Hourbound.Presentation.Player;

namespace Hourbound.Presentation.Feedback.Dash
{
    // 대시 시작 시 먼지 VFX를 1회 생성
    public sealed class DashDustSpawner : MonoBehaviour
    {
        [SerializeField] private DodgeController dodge;
        [SerializeField] private GameObject dustVfxPrefab;
        [SerializeField] private Transform spawnPoint; // 없다면 플레이어 위치에서 생성

        private void Awake()
        {
            if (dodge == null)
            {
                Debug.LogError("DashDustSpawner : DodgeController가 연결되지 않았습니다.");
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
            dodge.DodgeStarted += HandleDashStarted;
        }

        private void HandleDashStarted()
        {
            if (dustVfxPrefab == null) return;
            
            Vector3 pos = spawnPoint != null ? spawnPoint.position : dodge.transform.position;
            Instantiate(dustVfxPrefab, pos, Quaternion.identity);
        }
    }
}