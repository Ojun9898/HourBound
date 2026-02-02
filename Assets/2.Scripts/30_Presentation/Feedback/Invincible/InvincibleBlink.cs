using System.Collections;
using UnityEngine;
using Hourbound.Presentation.Player;

namespace Hourbound.Presentation.Feedback.Invincible
{
    public sealed class InvincibleBlink : MonoBehaviour
    {
        [SerializeField] private DodgeController dodge;
        [SerializeField] private Renderer[] renderers;
        
        [Min(0.01f)] [SerializeField] private float blinkInterval = 0.06f;
        
        private Coroutine _co;

        private void Reset()
        {
            dodge = GetComponentInParent<DodgeController>();
        }

        private void OnEnable()
        {
            if (dodge ==null) return;
            dodge.DodgeStarted  += OnDodgeStarted;
            dodge.DodgeEnded += OnDodgeEnded;
        }

        private void OnDisable()
        {
            if (dodge == null) return;
            dodge.DodgeStarted -= OnDodgeStarted;
            dodge.DodgeEnded -= OnDodgeEnded;
        }

        private void OnDodgeStarted()
        {
            StopBlink();
            _co = StartCoroutine(CoBlinkWhileInvincible());
        }

        private void OnDodgeEnded()
        {
            // 무적기 끝나기 전에 대시가 끝나도 계속 깜빡이게 하고 싶으면 
            // 여기서 끄지 말고 코루틴에서만 관리
        }

        private IEnumerator CoBlinkWhileInvincible()
        {
            while (dodge != null && dodge.IsInvincible)
            {
                SetRenderersVisible(false);
                yield return new WaitForSecondsRealtime(blinkInterval);
                SetRenderersVisible(true);
                yield return new WaitForSecondsRealtime(blinkInterval);
            }
            
            SetRenderersVisible(true);
            _co = null;
        }

        private void StopBlink()
        {
            if (_co != null)
            {
                StopCoroutine(_co);
                _co = null;
            }
        }

        private void SetRenderersVisible(bool visible)
        {
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].enabled = visible;
        }
    }
}