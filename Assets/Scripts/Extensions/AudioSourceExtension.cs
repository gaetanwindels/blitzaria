using TMPro.Examples;
using UnityEngine;

namespace Extensions
{
    public static class AudioSourceExtension
    {
        private static void SetCutOffFrequency(AudioSource audioSource, bool isInWater = false)
        {
            audioSource.pitch = 1;
            var lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter)
            {
                lowPassFilter.cutoffFrequency = isInWater ? 2800f : 10000f;
            }
        }

        public static void PlayWithRandomPitch(this AudioSource audioSource,
            bool isInWater = false, float minPitch = 1f, float maxPitch = 2f)
        {
            SetCutOffFrequency(audioSource, isInWater);
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.Play();
        }

        public static void PlayClip(this AudioSource audioSource, AudioClip clip, bool isInWater = false)
        {
            SetCutOffFrequency(audioSource, isInWater);
            audioSource.clip = clip;
            audioSource.Play();
        }

        public static void PlayClipWithRandomPitch(this AudioSource audioSource, AudioClip clip, bool isInWater = false,
            float minPitch = 1f,
            float maxPitch = 2f)
        {
            audioSource.clip = clip;
            PlayWithRandomPitch(audioSource, isInWater, minPitch, maxPitch);
        }
    }
}