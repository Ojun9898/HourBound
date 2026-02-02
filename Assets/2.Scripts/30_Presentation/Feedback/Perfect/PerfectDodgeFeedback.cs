using System;
using UnityEngine;
using Hourbound.Presentation.Combat;

namespace Hourbound.Presentation.Feedback
{
    // 퍼펙트 닷지 성공 시 링 VFX와 SFX를 실행
    public sealed class PerfectDodgeFeedback : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private PlayerHitReceiver hitReceiver;
        [SerializeField] private Transform spawnPoint; // 없으면 플레이어 위치에서 생성
        
        [Header("VFX/SFX")]
        [SerializeField] private GameObject ringVfxPrefab;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip perfectSfx;
        [Range(0f, 1f)] [SerializeField] private float volume = 1f;

        private void Awake()
        {
            if (hitReceiver == null)
            {
                Debug.LogError("PerfectDodgeFeedback : PlayerHitReceiver가 연결되지 않았습니다.");
                enabled = false;
                return;
            }
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            hitReceiver.PerfectDodged += OnPerfectDodged;
        }

        private void OnDisable()
        {
            hitReceiver.PerfectDodged -= OnPerfectDodged;
        }

        private void OnPerfectDodged()
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : hitReceiver.transform.position;
            
            if (ringVfxPrefab != null)
                Instantiate(ringVfxPrefab, pos, Quaternion.identity);
            
            if (audioSource != null)
                audioSource.PlayOneShot(perfectSfx, volume);
        }
    }
}