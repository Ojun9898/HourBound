using System;
using UnityEngine;

namespace Hourbound.Presentation.Combat
{
    // 매우 단순한 적 체럭 컴포넌트
    // 사망 시 이벤트만 발생시키고, 보상 / 드랍 / 연출은 외부에서 처리한다.
    public sealed class EnemyHealth : MonoBehaviour
    {
        [Header("기본 설정")] 
        [Min(1)] [SerializeField] private int maxHp = 10;
        
        public int CurrentHp { get; private set; }
        
        // 적이 사망했을 때 발생(누가 죽었는지 전달)
        public event Action<EnemyHealth> Died;

        private bool _isDead;

        private void Awake()
        {
            CurrentHp = maxHp;
        }

        public void TakeDamage(int amount)
        {
            if (_isDead) return;
            if (amount <= 0) return;

            CurrentHp = Mathf.Max(0, CurrentHp - amount);

            if (CurrentHp <= 0)
            {
                _isDead = true;
                Died?.Invoke(this);
            }
        }
    }
}
