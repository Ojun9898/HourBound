using System;
using UnityEngine;

namespace Hourbound.Presentation.Feedback
{
    // 간단 SFX 서비스(PlayOneShot 사용)
    public sealed class SfxService : MonoBehaviour
    {
        [SerializeField] private AudioSource audiosource;

        private void Awake()
        {
            if (audiosource == null)
            {
                audiosource = GetComponent<AudioSource>();
                if (audiosource == null)
                {
                    Debug.LogError("SfxService : AudioSource가 없습니다.", this);
                    enabled = false;
                }
            }
        }

        public void Play(AudioClip clip, float volume = 1f)
        {
            if (!enabled) return;
            if (clip == null) return;
            audiosource.PlayOneShot(clip, volume);
        }
    }
}