using System;
using System.Collections;
using UnityEngine;

namespace Hourbound.Presentation.Enemy.Visual
{
    public sealed class EnemyAnimatorResetOnSpawn : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private void Reset()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (animator == null) return;
            StartCoroutine(CoResetNextFrame());
        }

        private IEnumerator CoResetNextFrame()
        {
            yield return null;
            
            animator.Rebind();
            animator.ApplyBuiltinRootMotion();
            animator.Update(0f);
        }
    }
}