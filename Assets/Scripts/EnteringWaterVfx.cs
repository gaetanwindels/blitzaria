using System;
using UnityEngine;

public class EnteringWaterVfx : MonoBehaviour
{
    [SerializeField] private GameObject enteringWaterParticles;

    private bool hasTouchedWater = false;
    private Collider2D collider;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        return;
        
        if (other.gameObject.GetComponent<WaterDetector>())
        {
            var vfxCollider = GetComponent<Collider2D>();
            if (other.bounds.max.y > vfxCollider.bounds.max.y)
            {
                return;
            }
            
            var paticleSystem = enteringWaterParticles.GetComponent<ParticleSystem>();
            var triggerModule = paticleSystem.trigger;
            triggerModule.outside = ParticleSystemOverlapAction.Kill;
            triggerModule.enabled = true;
            triggerModule.SetCollider(0, other.gameObject.GetComponent<Collider2D>());
            paticleSystem.Play();
        }
    }

    private void Start()
    {
        collider = GetComponentInParent<Collider2D>(false);
    }

    private void Update()
    {
        if (!collider)
        {
            return;
        }
        
        var previousTouchedWater = hasTouchedWater;
        hasTouchedWater = collider.IsTouchingLayers(LayerMask.GetMask("Water Area"));
        if (!previousTouchedWater && hasTouchedWater)
        {
            var waterCollider = FindFirstObjectByType<WaterDetector>().gameObject.GetComponent<Collider2D>();
            
            if (collider.bounds.max.y < waterCollider.bounds.max.y)
            {
                return;
            }
            
            var paticleSystem = enteringWaterParticles.GetComponent<ParticleSystem>();
            var triggerModule = paticleSystem.trigger;
            triggerModule.outside = ParticleSystemOverlapAction.Kill;
            triggerModule.enabled = true;

            triggerModule.SetCollider(0, waterCollider);
            paticleSystem.Play();
        }
        
    }
}
