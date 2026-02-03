using UnityEngine;

namespace Hourbound.Presentation.Combat.Hitbox
{
    public sealed class PlayerHurtbox : MonoBehaviour
    {
        [SerializeField] private PlayerHitReceiver receiver;
        
        [Header("Debug")]
        [SerializeField] private bool logOnTrigger = true;

        private void Reset()
        {
            receiver = GetComponentInParent<PlayerHitReceiver>();
        }

        private void Awake()
        {
            if (receiver == null)
                receiver = GetComponentInParent<PlayerHitReceiver>();
            
            if (receiver == null)
                Debug.LogError("PlayerHurtbox : PlayerHitReceiver를 찾지 못했습니다.", this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (logOnTrigger)
                Debug.Log($"[HURTBOX] Hit by : {other.name} / layer = {other.gameObject.layer}", this);
            
            if (receiver == null) return;
            receiver.ReceiveHit(other.gameObject);
        }
    }
}