using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils
{
    public static void PlaySound(GameObject gameObject)
    {
        var audioSource = gameObject.GetComponent<AudioSource>();
        var audioFilter = gameObject.GetComponent<AudioLowPassFilter>();
        var collider = gameObject.GetComponent<Collider2D>();

        if (audioSource == null)
        {
            return;
        }

        var isTouchingWater = false;

        if (collider != null) {
            isTouchingWater = collider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        } 

        if (audioFilter != null)
        {
            audioFilter.cutoffFrequency = isTouchingWater ? 2800f : 10000f;
            audioSource.pitch = isTouchingWater ? 1.2f : 1f;
        }
        
        audioSource.Play();
    }
}
