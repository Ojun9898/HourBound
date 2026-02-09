using System;
using UnityEngine;
using Hourbound.Presentation.Combat;

namespace Hourbound.Presentation.Enemy.Lifecycle
{
    /// <summary>
    /// 적이 처치될 시 발생하는 이벤트(EnemyHealth.Died) 구독 스크립트
    /// - 적 처치시 SetActive(false) or Destroy(gameObject)
    /// </summary>
    public sealed class EnemyDeathDespawn : MonoBehaviour
    {
        [SerializeField] EnemyHealth health;
        
        [Header("Despawn")]
        [SerializeField] private bool destroyObject = false;
        [Min(0f)] [SerializeField] private float delaySeconds = 0f;

        private void Reset()
        {
            health = GetComponent<EnemyHealth>();
        }

        private void OnEnable()
        {
            if (health == null) health = GetComponent<EnemyHealth>();
            if (health != null) health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null) health.Died -= OnDied;
        }

        private void OnDied(EnemyHealth _)
        {
            if (delaySeconds > 0f) Invoke(nameof(DespawnNow), delaySeconds);
            else DespawnNow();
        }

        private void DespawnNow()
        {
            if (destroyObject)
            {
                Destroy(gameObject);
                return;
            }
            
            // 풀링 사용
            gameObject.SetActive(false);
        }
    }
}