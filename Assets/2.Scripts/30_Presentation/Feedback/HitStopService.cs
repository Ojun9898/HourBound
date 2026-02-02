using System.Collections;
using UnityEngine;

namespace Hourbound.Presentation.Feedback
{
    // 히트스톱 : 타임스케일을 건드리지 않고, 대상 Animator만 멈춘다.
    public sealed class HitStopService : MonoBehaviour
    {
        public void StopAnimator(Animator animator, float seconds)
        {
            if (animator == null) return;
            if (seconds <= 0) return;
            StartCoroutine(CoStop(animator, seconds));
        }

        private IEnumerator CoStop(Animator animator, float seconds)
        {
            float prev = animator.speed;
            animator.speed = 0;
            
            float end = UnityEngine.Time.realtimeSinceStartup + seconds;
            while (UnityEngine.Time.realtimeSinceStartup < end)
                yield return null;
            
            if (animator != null)
                animator.speed = prev;
        }
    }
}